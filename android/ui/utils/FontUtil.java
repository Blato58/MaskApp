package cn.com.heaton.shiningmask.ui.utils;

import android.graphics.Typeface;
import android.widget.EditText;
import android.widget.TextView;
import cn.com.heaton.shiningmask.base.App;

/* JADX INFO: loaded from: classes.dex */
public class FontUtil {
    private static Typeface jd_led5;
    private static Typeface typeface1248;
    private static Typeface typeface1456;
    private static Typeface typeface1664;
    private static Typeface typeface536;
    private static Typeface typefaceBold1664 = Typeface.createFromAsset(App.getInstance().getBaseContext().getAssets(), "fonts/bold16.TTF");

    public static Typeface getTypeface536() {
        return typeface536;
    }

    public static Typeface getTypeface1248() {
        return typeface1248;
    }

    public static Typeface getTypeface1456() {
        return typeface1456;
    }

    public static void setjd_led5(TextView textView) {
        textView.setTypeface(jd_led5);
    }

    public static void setjd_led5(EditText editText) {
        editText.setTypeface(jd_led5);
    }

    public static void setTypeface536(TextView textView) {
        textView.setTypeface(typeface536);
    }

    public static void setTypeface536(EditText editText) {
        editText.setTypeface(typeface536);
    }

    public static void setTypeface1248(TextView textView) {
        textView.setTypeface(typeface1248);
    }

    public static void setTypeface1248(EditText editText) {
        editText.setTypeface(typeface1248);
    }

    public static void setTypeface1456(TextView textView) {
        textView.setTypeface(typeface1456);
    }

    public static void setTypeface1456(EditText editText) {
        editText.setTypeface(typeface1456);
    }

    public static void setTypeface664(EditText editText) {
        editText.setTypeface(typeface1664);
    }

    public static Typeface getTypeface1664() {
        return typeface1664;
    }

    public static void setTypeface1664(Typeface typeface) {
        typeface1664 = typeface;
    }

    public static Typeface getTypefaceBold1664() {
        return typefaceBold1664;
    }

    public static void setTypefaceBold1664(Typeface typeface) {
        typefaceBold1664 = typeface;
    }
}