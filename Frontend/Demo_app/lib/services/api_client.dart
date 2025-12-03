import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../models/api_response.dart';
import '../models/feature_action.dart';

const String _defaultBaseUrl = String.fromEnvironment(
  'API_BASE_URL',
  defaultValue:
      kIsWeb ? 'http://localhost:8080/api' : 'http://10.0.2.2:8080/api',
);

const _requestTimeout = Duration(seconds: 15);

class ApiClient {
  ApiClient({this.baseUrl = _defaultBaseUrl, http.Client? client})
      : _client = client ?? http.Client();

  final String baseUrl;
  final http.Client _client;

  Future<ApiResponse> fetchStatus(FeatureAction feature) async {
    if (feature.statusPath == null) {
      return const ApiResponse(
        message: 'Nincs állapot végpont, futtasd a műveletet.',
      );
    }

    final uri = feature.buildUri(baseUrl, forStatus: true);

    try {
      final response = await _client.get(uri).timeout(_requestTimeout);
      return _parseBody(
        response,
        fallback: 'Állapot lekérdezve (${response.statusCode}).',
      );
    } catch (error, stackTrace) {
      debugPrint('Status fetch failed: $error\n$stackTrace');
      return ApiResponse(message: 'Nem sikerült lekérdezni: $error');
    }
  }

  Future<ApiResponse> triggerAction(
    FeatureAction feature, {
    Map<String, dynamic>? payloadOverride,
  }) async {
    final uri = feature.buildUri(baseUrl);
    final payload = payloadOverride ?? feature.payload;

    try {
      final response = await _switchRequest(uri, feature.method, payload);

      return _parseBody(
        response,
        fallback: 'Válaszkód: ${response.statusCode}',
      );
    } catch (error, stackTrace) {
      debugPrint('Action failed: $error\n$stackTrace');
      return ApiResponse(message: 'Nem sikerült végrehajtani: $error');
    }
  }

  Future<http.Response> _switchRequest(
    Uri uri,
    HttpMethod method,
    Map<String, dynamic>? payload,
  ) {
    final hasBody = payload != null;
    final body = hasBody ? jsonEncode(payload) : null;
    final headers = hasBody ? {'Content-Type': 'application/json'} : null;

    switch (method) {
      case HttpMethod.post:
        return _client
            .post(uri, headers: headers, body: body)
            .timeout(_requestTimeout);
      case HttpMethod.put:
        return _client
            .put(uri, headers: headers, body: body)
            .timeout(_requestTimeout);
      case HttpMethod.delete:
        return _client
            .delete(uri, headers: headers, body: body)
            .timeout(_requestTimeout);
      case HttpMethod.get:
        return _client.get(uri, headers: headers).timeout(_requestTimeout);
    }
  }

  ApiResponse _parseBody(http.Response response, {required String fallback}) {
    final isError = response.statusCode >= 400;
    if (response.body.isEmpty) {
      final emptyMessage = isError
          ? 'Hiba (${response.statusCode}): ${response.reasonPhrase ?? fallback}'
          : fallback;
      return ApiResponse(message: emptyMessage);
    }

    try {
      final decoded = jsonDecode(response.body);
      if (decoded is Map<String, dynamic>) {
        final map = decoded.cast<String, dynamic>();
        final message = (map['message'] ??
                map['status'] ??
                map['error'] ??
                map['detail'] ??
                map['title'] ??
                fallback)
            .toString();
        final resolvedMessage = isError
            ? 'Hiba (${response.statusCode}): $message'
            : message;
        return ApiResponse(message: resolvedMessage, data: map);
      }
      if (decoded is List<dynamic>) {
        final resolvedMessage = isError
            ? 'Hiba (${response.statusCode}): Lista érkezett a szervertől.'
            : 'Lista érkezett a szervertől.';
        return ApiResponse(
          message: resolvedMessage,
          data: {'items': decoded},
        );
      }
      final resolvedMessage = isError
          ? 'Hiba (${response.statusCode}): $decoded'
          : decoded.toString();
      return ApiResponse(message: resolvedMessage);
    } catch (_) {
      final resolvedMessage = isError
          ? 'Hiba (${response.statusCode}): ${response.body}'
          : response.body;
      return ApiResponse(message: resolvedMessage);
    }
  }

  void dispose() {
    _client.close();
  }
}
