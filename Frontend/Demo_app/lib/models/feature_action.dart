enum HttpMethod { get, post, put, delete }

class FeatureAction {
  const FeatureAction({
    required this.title,
    required this.description,
    required this.path,
    this.statusPath,
    this.payload,
    this.method = HttpMethod.get,
    this.actionLabel = 'Futtatás',
  });

  final String title;
  final String description;
  final String path;
  final String? statusPath;
  final Map<String, dynamic>? payload;
  final HttpMethod method;
  final String actionLabel;

  Uri buildUri(String baseUrl, {bool forStatus = false}) {
    final resolvedPath = forStatus && statusPath != null ? statusPath! : path;
    return Uri.parse('$baseUrl$resolvedPath');
  }
}
