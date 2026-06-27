package cn.com.heaton.shiningmask.ui.widget.loopviewpager;

import android.content.Context;
import android.util.DisplayMetrics;
import android.view.WindowManager;

/* JADX INFO: loaded from: classes.dex */
public class ScreenUtils {
    private static int SCREEN_HEIGHT;
    private static int SCREEN_WIDTH;

    public static int getScreenWidth(Context context) {
        return getScreenWidth(context, false);
    }

    public static int getScreenWidth(Context context, boolean z) {
        int i;
        if (!z && (i = SCREEN_WIDTH) > 0) {
            return i;
        }
        WindowManager windowManager = (WindowManager) context.getSystemService("window");
        DisplayMetrics displayMetrics = new DisplayMetrics();
        windowManager.getDefaultDisplay().getMetrics(displayMetrics);
        if (z) {
            return displayMetrics.widthPixels;
        }
        int i2 = displayMetrics.widthPixels;
        SCREEN_WIDTH = i2;
        return i2;
    }

    public static int getScreenHeight(Context context) {
        return getScreenHeight(context, false);
    }

    public static int getScreenHeight(Context context, boolean z) {
        int i;
        if (!z && (i = SCREEN_HEIGHT) > 0) {
            return i;
        }
        WindowManager windowManager = (WindowManager) context.getSystemService("window");
        DisplayMetrics displayMetrics = new DisplayMetrics();
        windowManager.getDefaultDisplay().getMetrics(displayMetrics);
        if (z) {
            return displayMetrics.heightPixels;
        }
        int i2 = displayMetrics.heightPixels;
        SCREEN_HEIGHT = i2;
        return i2;
    }

    public static int dpToPxInt(Context context, float f) {
        return (int) (dpToPx(context, f) + 0.5f);
    }

    public static float dpToPx(Context context, float f) {
        if (context == null) {
            return -1.0f;
        }
        return f * context.getResources().getDisplayMetrics().density;
    }
}