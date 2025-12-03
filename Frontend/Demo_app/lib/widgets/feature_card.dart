import 'package:flutter/material.dart';

import '../models/feature_action.dart';

class FeatureCard extends StatelessWidget {
  const FeatureCard({
    super.key,
    required this.feature,
    required this.statusFuture,
    required this.onRun,
    required this.onRefresh,
  });

  final FeatureAction feature;
  final Future<String> statusFuture;
  final VoidCallback onRun;
  final VoidCallback onRefresh;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    feature.title,
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                ),
                Chip(
                  label: Text(feature.method.name.toUpperCase()),
                  backgroundColor:
                      Theme.of(context).colorScheme.primaryContainer,
                ),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              feature.description,
              style: Theme.of(context).textTheme.bodyMedium,
            ),
            const SizedBox(height: 12),
            FutureBuilder<String>(
              future: statusFuture,
              builder: (context, snapshot) {
                if (snapshot.connectionState == ConnectionState.waiting) {
                  return const Row(
                    children: [
                      SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
                      SizedBox(width: 8),
                      Text('Állapot lekérdezése...'),
                    ],
                  );
                }
                if (snapshot.hasError) {
                  return Text(
                    'Hiba: ${snapshot.error}',
                    style:
                        TextStyle(color: Theme.of(context).colorScheme.error),
                  );
                }
                final status = snapshot.data ?? 'Nincs válasz';
                return Text(
                  status,
                  style:
                      TextStyle(color: Theme.of(context).colorScheme.secondary),
                );
              },
            ),
            const SizedBox(height: 12),
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                OutlinedButton.icon(
                  onPressed: onRefresh,
                  icon: const Icon(Icons.refresh),
                  label: const Text('Állapot'),
                ),
                const SizedBox(width: 8),
                ElevatedButton.icon(
                  onPressed: onRun,
                  icon: const Icon(Icons.play_arrow),
                  label: Text(feature.actionLabel),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
