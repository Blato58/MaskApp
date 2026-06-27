package cn.com.heaton.shiningmask.ui.utils;

import android.app.Activity;
import android.content.Context;
import android.widget.Toast;
import cn.com.heaton.shiningmask.base.App;

/* JADX INFO: loaded from: classes.dex */
public final class ToastUtil {
    private static Toast mToast;

    public static void showToast(String str) {
        Toast toast = mToast;
        if (toast == null) {
            mToast = Toast.makeText(App.getInstance(), str, 0);
        } else {
            toast.setText(str);
            mToast.setDuration(0);
        }
        mToast.show();
    }

    public static void showToast(int i) {
        Toast toast = mToast;
        if (toast == null) {
            mToast = Toast.makeText(App.getInstance(), i, 0);
        } else {
            toast.setText(i);
            mToast.setDuration(0);
        }
        mToast.show();
    }

    public static void cancelToast() {
        Toast toast = mToast;
        if (toast != null) {
            toast.cancel();
        }
    }

    public static void info(Activity activity, int i) {
        Toast.makeText(activity, i, 0).show();
    }

    public static void info(Activity activity, String str) {
        Toast.makeText(activity, str, 0).show();
    }

    public static void info(Context context, String str) {
        Toast.makeText(context, str, 0).show();
    }
}