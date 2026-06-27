package cn.com.heaton.shiningmask.ui.utils;

import android.content.Context;

/* JADX INFO: loaded from: classes.dex */
public class DensityUtil {
    public static float dp2px(Context context, float f) {
        if (context == null) {
            return -1.0f;
        }
        return (f * context.getResources().getDisplayMetrics().density) + 0.5f;
    }

    public static float px2dp(Context context, float f) {
        if (context == null) {
            return -1.0f;
        }
        return (f / context.getResources().getDisplayMetrics().density) + 0.5f;
    }

    public static float getWidth(Context context) {
        return context.getResources().getDisplayMetrics().widthPixels;
    }

    public static float getHeight(Context context) {
        return context.getResources().getDisplayMetrics().heightPixels;
    }
}