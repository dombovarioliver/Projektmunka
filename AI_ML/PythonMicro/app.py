from fastapi import FastAPI
from pydantic import BaseModel
import joblib
import pandas as pd

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

workout_split_model = joblib.load("workout_split_xgb_model.pkl")
WORKOUT_FEATURE_COLS = [
    "gender",
    "age",
    "goal_type",
    "activity_level",
    "experience",
    "days_per_week",
    "equipment_level"
]

class WorkoutInput(BaseModel):
    gender: float
    age: float
    goal_type: float
    activity_level: float
    experience: float
    days_per_week: float
    equipment_level: float

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

class WorkoutSplitResponse(BaseModel):
    split_type: int
    split_name: str


app = FastAPI(
    title="DiplomaFit ML API",
    description="Diet (makró) és edzéssplit (FullBody / UL / PPL) ML végpontok",
    version="1.0.0"
)

@app.get("/")
def root():
    return {
        "status": "ok",
        "msg": "DiplomaFit ML API online",
        "endpoints": [
            "/predict",
            "/workout-split/predict"
        ]
    }

@app.post("/predict")
def predict(input_data: DietInput):
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

@app.post("/workout-split/predict")
def predict_workout_split(input_data: WorkoutInput):
    row = [getattr(input_data, c) for c in WORKOUT_FEATURE_COLS]
    df = pd.DataFrame([row], columns=WORKOUT_FEATURE_COLS)

    pred = workout_split_model.predict(df)[0]
    split_type = int(pred)

    split_names = {
        0: "FullBody",
        1: "UpperLower",
        2: "PushPullLegs"
    }
    split_name = split_names.get(split_type, "Unknown")

    return WorkoutSplitResponse(
        split_type=split_type,
        split_name=split_name
    )