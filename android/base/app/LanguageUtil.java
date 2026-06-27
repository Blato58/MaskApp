package cn.com.heaton.shiningmask.base.app;

import android.content.Context;
import android.content.res.Configuration;
import android.text.TextUtils;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.SPUtils;
import java.util.Locale;

/* JADX INFO: loaded from: classes.dex */
public class LanguageUtil {
    public static void applyLanguage(Context context, String str) {
        context.getResources().getConfiguration().setLocale(SupportLanguageUtil.getSupportLanguage(str));
    }

    public static Context attachBaseContext(Context context, String str) {
        return createConfigurationResources(context, str);
    }

    public static String getSaveLanguage(Context context) {
        String language = SupportLanguageUtil.getSystemPreferredLanguage().getLanguage();
        if ("zh".equals(language)) {
            String country = SupportLanguageUtil.getSystemPreferredLanguage().getCountry();
            LogUtil.e("==country=" + country);
            if (country.equals("TW") || country.equals("HK")) {
                language = "zh_TW";
            }
        }
        String str = (String) SPUtils.get(context, C.SP.LANGUAGE, language);
        return SupportLanguageUtil.isSupportLanguage(str) ? str : "en";
    }

    private static Context createConfigurationResources(Context context, String str) {
        Locale supportLanguage;
        Configuration configuration = context.getResources().getConfiguration();
        if (TextUtils.isEmpty(str)) {
            supportLanguage = SupportLanguageUtil.getSystemPreferredLanguage();
        } else {
            supportLanguage = SupportLanguageUtil.getSupportLanguage(str);
        }
        configuration.setLocale(supportLanguage);
        return context.createConfigurationContext(configuration);
    }
}