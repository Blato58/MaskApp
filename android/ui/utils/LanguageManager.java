package cn.com.heaton.shiningmask.ui.utils;

import android.content.Context;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.util.DisplayMetrics;
import java.util.Locale;

/* JADX INFO: loaded from: classes.dex */
public class LanguageManager {
    public static final String LANGUAGE_AUTO = "auto";
    public static final String LANGUAGE_CHINESE_SIMPLIFIED = "zh";
    public static final String LANGUAGE_CHINESE_TRADITIONAL = "zh_TW";
    public static final String LANGUAGE_DE = "de";
    public static final String LANGUAGE_ENGLISH = "en";
    public static final String LANGUAGE_ES = "es";
    public static final String LANGUAGE_FR = "fr";
    public static final String LANGUAGE_JA = "ja";
    public static final String LANGUAGE_KO = "ko";
    public static final String LANGUAGE_PT = "pt";
    public static final String LANGUAGE_RU = "ru";
    private static final String sp_key = "language";

    public static void initLanguage(Context context) {
        String saveLanguage = getSaveLanguage(context);
        if (saveLanguage == null) {
            saveLanguage = "auto";
        }
        setLanguage(context, saveLanguage);
    }

    public static void setLanguage(Context context, String str) {
        Configuration configuration;
        LogUtil.e("设置语言:" + str);
        if (context == null || str == null) {
            return;
        }
        Resources resources = context.getResources();
        configuration = resources.getConfiguration();
        DisplayMetrics displayMetrics = resources.getDisplayMetrics();
        str.hashCode();
        switch (str) {
            case "de":
                configuration.locale = Locale.GERMANY;
                break;
            case "en":
                configuration.locale = Locale.ENGLISH;
                break;
            case "es":
                configuration.locale = new Locale("es");
                break;
            case "fr":
                configuration.locale = Locale.FRANCE;
                break;
            case "ja":
                configuration.locale = Locale.JAPAN;
                break;
            case "ko":
                configuration.locale = Locale.KOREA;
                break;
            case "pt":
                configuration.locale = new Locale("pt");
                break;
            case "ru":
                configuration.locale = new Locale("ru");
                break;
            case "zh":
                configuration.locale = Locale.SIMPLIFIED_CHINESE;
                break;
            case "auto":
                configuration.locale = Locale.getDefault();
                break;
            case "zh_TW":
                configuration.locale = Locale.TRADITIONAL_CHINESE;
                break;
            default:
                configuration.locale = Locale.getDefault();
                break;
        }
        resources.updateConfiguration(configuration, displayMetrics);
    }

    public static boolean saveLanguage(Context context, String str) {
        if (context == null || str == null) {
            return false;
        }
        return context.getSharedPreferences("language", 0).edit().putString("language", str).commit();
    }

    public static String getSaveLanguage(Context context) {
        if (context == null) {
            return null;
        }
        return context.getSharedPreferences("language", 0).getString("language", null);
    }
}