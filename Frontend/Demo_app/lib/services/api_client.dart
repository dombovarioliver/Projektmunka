import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

import '../models/feature_action.dart';

const String _defaultBaseUrl = String.fromEnvironment(
  'API_BASE_URL',
  defaultValue: kIsWeb
      ? 'http://localhost:8080/api'
      : 'http://10.0.2.2:8080/api',
);

class ApiClient {
  ApiClient({this.baseUrl = _defaultBaseUrl, http.Client? client})
    : _client = client ?? http.Client();

  final String baseUrl;
  final http.Client _client;

  Future<String> fetchStatus(FeatureAction feature) async {
    if (feature.statusPath == null) {
      return 'Nincs állapot végpont, futtasd a műveletet.';
    }

    final uri = feature.buildUri(baseUrl, forStatus: true);

    try {
      final response = await _client.get(uri);
      return _parseBody(
        response,
        fallback: 'Állapot lekérdezve (${response.statusCode}).',
      );
    } catch (error, stackTrace) {
      debugPrint('Status fetch failed: $error\n$stackTrace');
      return 'Nem sikerült lekérdezni: $error';
    }
  }

  Future<String> triggerAction(
    FeatureAction feature, {
    Map<String, dynamic>? payloadOverride,
  }) async {
    final uri = feature.buildUri(baseUrl);
    final payload = payloadOverride ?? feature.payload;
    final uri = feature.buildUri(baseUrl);

    try {
      final response = await _switchRequest(uri, feature.method, payload);

      return _parseBody(
        response,
        fallback: 'Válaszkód: ${response.statusCode}',
      );
    } catch (error, stackTrace) {
      debugPrint('Action failed: $error\n$stackTrace');
      return 'Nem sikerült végrehajtani: $error';
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
        return _client.post(uri, headers: headers, body: body);
      case HttpMethod.put:
        return _client.put(uri, headers: headers, body: body);
      case HttpMethod.delete:
        return _client.delete(uri, headers: headers, body: body);
      case HttpMethod.get:
        return _client.get(uri, headers: headers);
    }
  }

  String _parseBody(http.Response response, {required String fallback}) {
    if (response.body.isEmpty) {
      return fallback;
    }

    try {
      final decoded = jsonDecode(response.body);
      if (decoded is Map<String, dynamic> && decoded['message'] is String) {
        return decoded['message'] as String;
      }
      return decoded.toString();
    } catch (_) {
      return response.body;
    }
  }

  void dispose() {
    _client.close();
  }
}
