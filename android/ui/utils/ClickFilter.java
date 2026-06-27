package cn.com.heaton.shiningmask.ui.utils;

import android.util.Log;

/* JADX INFO: loaded from: classes.dex */
public class ClickFilter {
    private static final long INTERVAL = 500;
    private static long lastClickTime;

    public static boolean filter() {
        long jCurrentTimeMillis = System.currentTimeMillis();
        long j = jCurrentTimeMillis - lastClickTime;
        if (0 < j && j < INTERVAL) {
            Log.e("blemanager", "ClickFilter: +++++");
            return true;
        }
        lastClickTime = jCurrentTimeMillis;
        return false;
    }
}