package cn.com.heaton.shiningmask.ui.utils;

import android.util.Log;
import java.util.Locale;

/* JADX INFO: loaded from: classes.dex */
public class LogUtil {
    private static boolean ISLOG = false;

    private static String getTag() {
        StackTraceElement[] stackTrace = new Throwable().fillInStackTrace().getStackTrace();
        for (int i = 2; i < stackTrace.length; i++) {
            if (!stackTrace[i].getClass().equals(LogUtil.class)) {
                String className = stackTrace[i].getClassName();
                return className.substring(className.lastIndexOf(46) + 1);
            }
        }
        return "";
    }

    private static String buildMessage(String str) {
        String methodName;
        StackTraceElement[] stackTrace = new Throwable().fillInStackTrace().getStackTrace();
        int i = 2;
        while (true) {
            if (i >= stackTrace.length) {
                methodName = "";
                break;
            }
            if (!stackTrace[i].getClass().equals(LogUtil.class)) {
                methodName = stackTrace[i].getMethodName();
                break;
            }
            i++;
        }
        return String.format(Locale.US, "[%d] %s: %s", Long.valueOf(Thread.currentThread().getId()), methodName, str);
    }

    public static void v(String str) {
        if (ISLOG) {
            Log.v(getTag(), buildMessage(str));
        }
    }

    public static void d(String str) {
        if (ISLOG) {
            Log.d(getTag(), buildMessage(str));
        }
    }

    public static void i(String str) {
        if (ISLOG) {
            Log.i(getTag(), buildMessage(str));
        }
    }

    public static void w(String str) {
        if (ISLOG) {
            Log.w(getTag(), buildMessage(str));
        }
    }

    public static void e(String str) {
        if (ISLOG) {
            Log.e(getTag(), buildMessage(str));
        }
    }
}