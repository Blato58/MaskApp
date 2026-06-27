package cn.com.heaton.shiningmask.ui.roteview;

import android.animation.TypeEvaluator;

/* JADX INFO: loaded from: classes.dex */
public class AngleEvaluator implements TypeEvaluator {
    @Override // android.animation.TypeEvaluator
    public Object evaluate(float f, Object obj, Object obj2) {
        float fFloatValue = ((Float) obj).floatValue();
        return Float.valueOf(fFloatValue + (f * (((Float) obj2).floatValue() - fFloatValue)));
    }
}