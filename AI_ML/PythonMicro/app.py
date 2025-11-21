from fastapi import FastAPI
from pydantic import BaseModel
import joblib
import pandas as pd

# 1) Modell betöltése
model = joblib.load("multioutput_xgb_model.pkl")
FEATURE_COLS = [
    "gender",
    "age",
    "height_cm",
    "weight_kg",
    "bodyfat_percent",
    "activity_level",
    "goal_type",
    "goal_delta_kg",
    "goal_time_weeks"
]

# 2) Feature sorrend – PONTOSAN úgy, ahogy tanítottad Jupyterben

# 3) Request body (amit kívülről kapni fog a modell)
class DietInput(BaseModel):
    gender: float
    age: float
    height_cm: float
    weight_kg: float
    bodyfat_percent: float
    activity_level: float
    goal_type: float
    goal_delta_kg: float
    goal_time_weeks: float

app = FastAPI()

@app.get("/")
def root():
    return {"status": "ok", "msg": "Diet ML model online"}

@app.post("/predict")
def predict(input_data: DietInput):
    # DietInput -> DataFrame, a tanításhoz használt sorrendben
    row = [getattr(input_data, c) for c in FEATURE_COLS]
    df = pd.DataFrame([row], columns=FEATURE_COLS)

    # Multi-output predikció: 6 értéket ad vissza
    preds = model.predict(df)[0]

    return {
        "calories_kcal": round(float(preds[0]),2),
        "protein_g": round(float(preds[1]),2),
        "carbs_g": round(float(preds[2]),2),
        "fat_g": round(float(preds[3]),2),
        "meals_per_day": round(float(preds[4]),0),
        "snacks_per_day": round(float(preds[5]),0),
    }