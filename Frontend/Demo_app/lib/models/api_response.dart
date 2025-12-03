class ApiResponse {
  const ApiResponse({required this.message, this.data});

  final String message;
  final Map<String, dynamic>? data;
}
