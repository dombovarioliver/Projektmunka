import 'package:flutter/material.dart';

class PlanResultDialog extends StatelessWidget {
  const PlanResultDialog({
    super.key,
    this.message,
    this.dietPlan,
    this.workoutPlan,
  });

  final String? message;
  final Map<String, dynamic>? dietPlan;
  final Map<String, dynamic>? workoutPlan;

  static Future<void> show(
    BuildContext context, {
    String? message,
    Map<String, dynamic>? dietPlan,
    Map<String, dynamic>? workoutPlan,
  }) {
    return showDialog(
      context: context,
      builder: (_) => PlanResultDialog(
        message: message,
        dietPlan: dietPlan,
        workoutPlan: workoutPlan,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Generált tervek'),
      content: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 520, maxHeight: 600),
        child: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (message != null && message!.isNotEmpty)
                Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: Text(
                    message!,
                    style: Theme.of(context).textTheme.bodyLarge,
                  ),
                ),
              if (dietPlan != null)
                _PlanSection(title: 'Étrend', data: dietPlan!),
              if (dietPlan != null && workoutPlan != null)
                const SizedBox(height: 12),
              if (workoutPlan != null)
                _PlanSection(title: 'Edzésterv', data: workoutPlan!),
              if (dietPlan == null && workoutPlan == null)
                const Text('Nem érkezett formázható terv a szervertől.'),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Bezárás'),
        ),
      ],
    );
  }
}

class _PlanSection extends StatelessWidget {
  const _PlanSection({required this.title, required this.data});

  final String title;
  final Map<String, dynamic> data;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: Theme.of(context).textTheme.titleMedium,
        ),
        const SizedBox(height: 8),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(12),
            child: _FormattedValue(value: data),
          ),
        ),
      ],
    );
  }
}

class _FormattedValue extends StatelessWidget {
  const _FormattedValue({required this.value});

  final dynamic value;

  @override
  Widget build(BuildContext context) {
    if (value is Map<String, dynamic>) {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          for (final entry in value.entries)
            Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '${entry.key}: ',
                    style: const TextStyle(fontWeight: FontWeight.w600),
                  ),
                  Expanded(child: _FormattedValue(value: entry.value)),
                ],
              ),
            ),
        ],
      );
    }

    if (value is List<dynamic>) {
      if (value.isEmpty) {
        return const Text('Nincs adat');
      }
      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          for (var i = 0; i < value.length; i++)
            Padding(
              padding: const EdgeInsets.only(bottom: 6),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('${i + 1}. ',
                      style: const TextStyle(fontWeight: FontWeight.bold)),
                  Expanded(child: _FormattedValue(value: value[i])),
                ],
              ),
            ),
        ],
      );
    }

    return Text(value.toString());
  }
}
