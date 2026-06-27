package cn.com.heaton.shiningmask.ui.utils;

import android.content.Context;

/* JADX INFO: loaded from: classes.dex */
public class DeviceNameUtil {
    private static final String sp_key = "device_name";

    public static boolean saveName(Context context, String str, String str2) {
        if (context == null || str == null || str2 == null) {
            return false;
        }
        return context.getSharedPreferences(sp_key, 0).edit().putString(str, str2).commit();
    }

    public static String getSaveName(Context context, String str) {
        if (context == null || str == null) {
            return null;
        }
        return context.getSharedPreferences(sp_key, 0).getString(str, null);
    }
}