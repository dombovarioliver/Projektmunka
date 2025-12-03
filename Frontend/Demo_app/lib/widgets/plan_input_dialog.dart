import 'package:flutter/material.dart';

class PlanInputDialog extends StatefulWidget {
  const PlanInputDialog({
    super.key,
    this.includeDiet = true,
    this.includeWorkout = true,
    this.initialDietValues,
    this.initialWorkoutValues,
  });

  final bool includeDiet;
  final bool includeWorkout;
  final Map<String, dynamic>? initialDietValues;
  final Map<String, dynamic>? initialWorkoutValues;

  static Future<Map<String, dynamic>?> show(
    BuildContext context, {
    bool includeDiet = true,
    bool includeWorkout = true,
    Map<String, dynamic>? initialDietValues,
    Map<String, dynamic>? initialWorkoutValues,
  }) {
    return showDialog<Map<String, dynamic>>(
      context: context,
      builder: (_) => PlanInputDialog(
        includeDiet: includeDiet,
        includeWorkout: includeWorkout,
        initialDietValues: initialDietValues,
        initialWorkoutValues: initialWorkoutValues,
      ),
    );
  }

  @override
  State<PlanInputDialog> createState() => _PlanInputDialogState();
}

class _PlanInputDialogState extends State<PlanInputDialog> {
  static const Map<String, dynamic> _defaultDietValues = {
    'gender': 0,
    'age': 30,
    'heightCm': 180,
    'weightKg': 82.5,
    'bodyfatPercent': 18,
    'activityLevel': 3,
    'goalType': 1,
    'goalDeltaKg': 5,
    'goalTimeWeeks': 8,
  };

  static const Map<String, dynamic> _defaultWorkoutValues = {
    'gender': 0,
    'age': 28,
    'goal_type': 2,
    'activity_level': 3,
    'experience': 2,
    'days_per_week': 4,
    'equipment_level': 1,
  };

  final Map<String, TextEditingController> _dietControllers = {};
  final Map<String, TextEditingController> _workoutControllers = {};

  @override
  void initState() {
    super.initState();
    if (widget.includeDiet) {
      _initialiseControllers(
        _dietControllers,
        _defaultDietValues,
        widget.initialDietValues,
      );
    }
    if (widget.includeWorkout) {
      _initialiseControllers(
        _workoutControllers,
        _defaultWorkoutValues,
        widget.initialWorkoutValues,
      );
    }
  }

  @override
  void dispose() {
    for (final controller in _dietControllers.values) {
      controller.dispose();
    }
    for (final controller in _workoutControllers.values) {
      controller.dispose();
    }
    super.dispose();
  }

  void _initialiseControllers(
    Map<String, TextEditingController> target,
    Map<String, dynamic> defaults,
    Map<String, dynamic>? overrides,
  ) {
    for (final entry in defaults.entries) {
      final value = (overrides ?? const {})[entry.key] ?? entry.value;
      target[entry.key] = TextEditingController(text: '$value');
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Add meg az adatokat'),
      content: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 480),
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (widget.includeDiet) _buildDietSection(),
              if (widget.includeWorkout) _buildWorkoutSection(),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Mégse'),
        ),
        ElevatedButton(onPressed: _submit, child: const Text('Küldés')),
      ],
    );
  }

  Widget _buildDietSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Étrendhez szükséges adatok',
          style: Theme.of(context).textTheme.titleMedium,
        ),
        const SizedBox(height: 8),
        _numberField(_dietControllers['gender']!, 'Nem (0=férfi,1=nő)'),
        _numberField(_dietControllers['age']!, 'Életkor'),
        _numberField(_dietControllers['heightCm']!, 'Magasság (cm)'),
        _numberField(_dietControllers['weightKg']!, 'Testsúly (kg)'),
        _numberField(_dietControllers['bodyfatPercent']!, 'Testzsír százalék'),
        _numberField(_dietControllers['activityLevel']!, 'Aktivitási szint'),
        _numberField(_dietControllers['goalType']!, 'Cél típus'),
        _numberField(
          _dietControllers['goalDeltaKg']!,
          'Cél tömeg változás (kg)',
        ),
        _numberField(
          _dietControllers['goalTimeWeeks']!,
          'Cél elérés ideje (hetek)',
        ),
        const SizedBox(height: 12),
      ],
    );
  }

  Widget _buildWorkoutSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Edzésterv adatai',
          style: Theme.of(context).textTheme.titleMedium,
        ),
        const SizedBox(height: 8),
        _numberField(_workoutControllers['gender']!, 'Nem (0=férfi,1=nő)'),
        _numberField(_workoutControllers['age']!, 'Életkor'),
        _numberField(_workoutControllers['goal_type']!, 'Cél típus'),
        _numberField(
          _workoutControllers['activity_level']!,
          'Aktivitási szint',
        ),
        _numberField(_workoutControllers['experience']!, 'Tapasztalat'),
        _numberField(
          _workoutControllers['days_per_week']!,
          'Edzésnapok hetente',
        ),
        _numberField(
          _workoutControllers['equipment_level']!,
          'Eszközök elérhetősége',
        ),
      ],
    );
  }

  Widget _numberField(TextEditingController controller, String label) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: TextField(
        controller: controller,
        keyboardType: TextInputType.number,
        decoration: InputDecoration(
          labelText: label,
          border: const OutlineInputBorder(),
        ),
      ),
    );
  }

  void _submit() {
    final result = <String, dynamic>{};
    if (widget.includeDiet) {
      result['dietInputs'] = {
        'gender': _parseNum(_dietControllers['gender']!.text),
        'age': _parseNum(_dietControllers['age']!.text),
        'heightCm': _parseNum(_dietControllers['heightCm']!.text),
        'weightKg': _parseNum(_dietControllers['weightKg']!.text),
        'bodyfatPercent': _parseNum(_dietControllers['bodyfatPercent']!.text),
        'activityLevel': _parseNum(_dietControllers['activityLevel']!.text),
        'goalType': _parseNum(_dietControllers['goalType']!.text),
        'goalDeltaKg': _parseNum(_dietControllers['goalDeltaKg']!.text),
        'goalTimeWeeks': _parseNum(_dietControllers['goalTimeWeeks']!.text),
      };
    }

    if (widget.includeWorkout) {
      result['workoutInputs'] = {
        'gender': _parseNum(_workoutControllers['gender']!.text),
        'age': _parseNum(_workoutControllers['age']!.text),
        'goal_type': _parseNum(_workoutControllers['goal_type']!.text),
        'activity_level': _parseNum(
          _workoutControllers['activity_level']!.text,
        ),
        'experience': _parseNum(_workoutControllers['experience']!.text),
        'days_per_week': _parseNum(_workoutControllers['days_per_week']!.text),
        'equipment_level': _parseNum(
          _workoutControllers['equipment_level']!.text,
        ),
      };
    }

    Navigator.of(context).pop(result);
  }

  num _parseNum(String value) {
    return num.tryParse(value) ?? 0;
  }
}
