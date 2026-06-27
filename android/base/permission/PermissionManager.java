package cn.com.heaton.shiningmask.base.permission;

import android.app.Activity;
import pub.devrel.easypermissions.EasyPermissions;

/* JADX INFO: loaded from: classes.dex */
public class PermissionManager {
    public static boolean checkPermission(Activity activity, String[] strArr) {
        return EasyPermissions.hasPermissions(activity, strArr);
    }

    public static void requestPermission(Activity activity, String str, int i, String[] strArr) {
        EasyPermissions.requestPermissions(activity, str, i, strArr);
    }
}