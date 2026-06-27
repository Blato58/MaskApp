package cn.com.heaton.shiningmask.ui.utils;

/* JADX INFO: loaded from: classes.dex */
public class TimeIntervalUtil {
    private static final long INTERVAL = 50;
    private static long lastClickTime;

    public static boolean filter() {
        long jCurrentTimeMillis = System.currentTimeMillis();
        long j = jCurrentTimeMillis - lastClickTime;
        if (0 < j && j < INTERVAL) {
            return true;
        }
        lastClickTime = jCurrentTimeMillis;
        return false;
    }
}