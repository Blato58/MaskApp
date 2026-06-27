package cn.com.heaton.shiningmask.base.app;

import android.os.LocaleList;
import cn.com.heaton.shiningmask.base.app.C;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class SupportLanguageUtil {
    private static Map<String, Locale> mSupportLanguages = new HashMap<String, Locale>(2) { // from class: cn.com.heaton.shiningmask.base.app.SupportLanguageUtil.1
        {
            put("zh", Locale.SIMPLIFIED_CHINESE);
            put("en", Locale.ENGLISH);
            put("zh_TW", Locale.TAIWAN);
            put("de", Locale.GERMANY);
            put(C.SP.LANGUAGE_HK, new Locale("zh", "HK"));
            put("ja", Locale.JAPAN);
            put("de", Locale.GERMANY);
            put("pt", new Locale("pt"));
            put("es", new Locale("es"));
            put("fr", Locale.FRANCE);
            put("ko", Locale.KOREA);
            put("ru", new Locale("ru"));
        }
    };

    public static boolean isSupportLanguage(String str) {
        return mSupportLanguages.containsKey(str);
    }

    public static Locale getSupportLanguage(String str) {
        if (isSupportLanguage(str)) {
            return mSupportLanguages.get(str);
        }
        return getSystemPreferredLanguage();
    }

    public static Locale getSystemPreferredLanguage() {
        return LocaleList.getDefault().get(0);
    }
}