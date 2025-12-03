import 'package:flutter/material.dart';

import '../models/feature_action.dart';
import '../services/api_client.dart';
import '../widgets/feature_card.dart';
import '../widgets/plan_input_dialog.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  late final ApiClient _apiClient;
  late final List<FeatureAction> _features;
  final Map<String, Future<String>> _statusFutures = {};

  @override
  void initState() {
    super.initState();
    _apiClient = ApiClient();
    _features = _buildFeatureList();
    for (final feature in _features) {
      _statusFutures[feature.path] = _apiClient.fetchStatus(feature);
    }
  }

  @override
  void dispose() {
    _apiClient.dispose();
    super.dispose();
  }

  List<FeatureAction> _buildFeatureList() {
    return [
      const FeatureAction(
        title: 'Étrend import',
        description:
            'Plan.csv és Case.csv betöltése a konténer docs mappájából.',
        path:
            '/import/plans-and-cases?planPath=/docs/Diet/Plan.csv&casePath=/docs/Diet/Case.csv',
        method: HttpMethod.post,
        actionLabel: 'Importálás',
      ),
      const FeatureAction(
        title: 'Alap ételek import',
        description: 'Foods.csv betöltése a diet modulhoz.',
        path: '/import/foods?path=/docs/Diet/Foods.csv',
        method: HttpMethod.post,
        actionLabel: 'Importálás',
      ),
      const FeatureAction(
        title: 'Gyakorlatok import',
        description: 'Exercises.csv betöltése az edzés modulhoz.',
        path: '/import/exercises?path=/docs/Workout/Exercises.csv',
        method: HttpMethod.post,
        actionLabel: 'Importálás',
      ),
      FeatureAction(
        title: 'Heti étrend generálása',
        description: 'Adj meg diet adatokat, majd generáljuk a heti étrendet.',
        path: '/planning/generate-weekly-diet-plan',
        payloadBuilder: (context) => PlanInputDialog.show(
          context,
          includeWorkout: false,
        ).then((result) => result?['dietInputs'] as Map<String, dynamic>?),
        statusPath:
            '/diet/cases/e35dc5ec-2bc0-4b33-a347-595e9b19d7d6/weekly-plan',
        method: HttpMethod.post,
        actionLabel: 'Generálás',
      ),
      FeatureAction(
        title: 'Heti edzésterv generálása',
        description: 'Állítsd be a szükséges paramétereket a heti edzéshez.',
        path: '/planning/workout/weekly-workout-plan',
        payloadBuilder: (context) => PlanInputDialog.show(
          context,
          includeDiet: false,
        ).then((result) => result?['workoutInputs'] as Map<String, dynamic>?),
        method: HttpMethod.post,
        actionLabel: 'Generálás',
      ),
      FeatureAction(
        title: 'Étrend + edzésterv generálása',
        description:
            'Kérd be külön a diet és workout inputokat és egy lépésben generáljuk mindkettőt.',
        path: '/planning/generate-diet-and-workout-plans',
        payloadBuilder: (context) => PlanInputDialog.show(context),
        method: HttpMethod.post,
        actionLabel: 'Generálás',
      ),
    ];
  }

  Future<String> _refreshStatus(FeatureAction feature) {
    final future = _apiClient.fetchStatus(feature);
    setState(() {
      _statusFutures[feature.path] = future;
    });
    return future;
  }

  Future<void> _triggerAction(FeatureAction feature) async {
    final payload = feature.payloadBuilder != null
        ? await feature.payloadBuilder!(context)
        : null;
    if (feature.payloadBuilder != null && payload == null) {
      return;
    }

    final future = _apiClient.triggerAction(
      feature,
      payloadOverride: payload ?? feature.payload,
    );
    setState(() {
      _statusFutures[feature.path] = future;
    });

    final result = await future;
    if (!mounted) return;
    if (result.isNotEmpty) {
      // ignore: use_build_context_synchronously
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(result)));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Funkció bemutató')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: ListView.separated(
          itemCount: _features.length,
          separatorBuilder: (_, __) => const SizedBox(height: 12),
          itemBuilder: (context, index) {
            final feature = _features[index];
            final statusFuture =
                _statusFutures[feature.path] ?? _refreshStatus(feature);

            return FeatureCard(
              feature: feature,
              statusFuture: statusFuture,
              onRefresh: () => _refreshStatus(feature),
              onRun: () => _triggerAction(feature),
            );
          },
        ),
      ),
    );
  }
}
