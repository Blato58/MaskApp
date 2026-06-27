package cn.com.heaton.shiningmask.ui.utils;

import android.util.Log;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.TimeZone;

/* JADX INFO: loaded from: classes.dex */
public class TimeUtils {
    public static String getNowTime() {
        return new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").format(new Date(System.currentTimeMillis()));
    }

    public static String getTimeString() {
        return new SimpleDateFormat("yyyy年MM月dd日 HH:mm:ss").format(Calendar.getInstance().getTime());
    }

    public static String dateToStamp(String str) {
        Date date;
        try {
            date = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").parse(str);
        } catch (ParseException e) {
            e.printStackTrace();
            date = null;
        }
        return String.valueOf(date.getTime());
    }

    public static String times(String str) {
        SimpleDateFormat simpleDateFormat = new SimpleDateFormat("yyyy年MM月dd日 HH时mm分");
        Long.valueOf(str).longValue();
        return simpleDateFormat.format(new Date(((long) Integer.parseInt(str)) * 1000));
    }

    public static String getLongTime(int i) {
        Calendar calendar = Calendar.getInstance();
        calendar.set(11, calendar.get(11) - i);
        SimpleDateFormat simpleDateFormat = new SimpleDateFormat("yyyy年MM月dd日 HH:mm:ss");
        Log.v("time", simpleDateFormat.format(calendar.getTime()));
        return simpleDateFormat.format(calendar.getTime());
    }

    public static boolean isDateOneBigger(String str, String str2) {
        Date date;
        SimpleDateFormat simpleDateFormat = new SimpleDateFormat("yyyy年MM月dd日 HH:mm:ss");
        Date date2 = null;
        try {
            date = simpleDateFormat.parse(str);
        } catch (ParseException e) {
            e = e;
            date = null;
        }
        try {
            date2 = simpleDateFormat.parse(str2);
        } catch (ParseException e2) {
            e = e2;
            e.printStackTrace();
        }
        if (date.getTime() > date2.getTime()) {
            return true;
        }
        date.getTime();
        date2.getTime();
        return false;
    }

    public static String Local2UTC() {
        SimpleDateFormat simpleDateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        simpleDateFormat.setTimeZone(TimeZone.getTimeZone("gmt"));
        return simpleDateFormat.format(new Date());
    }

    public static String utc2Local(String str) {
        Date date;
        SimpleDateFormat simpleDateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        simpleDateFormat.setTimeZone(TimeZone.getTimeZone("UTC"));
        try {
            date = simpleDateFormat.parse(str);
        } catch (ParseException e) {
            e.printStackTrace();
            date = null;
        }
        SimpleDateFormat simpleDateFormat2 = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        simpleDateFormat2.setTimeZone(TimeZone.getDefault());
        return simpleDateFormat2.format(Long.valueOf(date.getTime()));
    }
}