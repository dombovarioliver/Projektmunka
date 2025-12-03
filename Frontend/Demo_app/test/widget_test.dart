import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:demo_app/main.dart';

void main() {
  testWidgets('Home screen renders and shows available actions',
      (WidgetTester tester) async {
    await tester.pumpWidget(const DemoApp());

    expect(find.text('Funkció bemutató'), findsOneWidget);
    expect(find.byType(ElevatedButton), findsWidgets);

    await tester.pumpAndSettle();
    expect(find.textContaining('Állapot'), findsWidgets);
  });
}
