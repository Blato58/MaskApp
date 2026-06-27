package cn.com.heaton.shiningmask.ui.utils;

import android.text.LoginFilter;
import android.util.Log;

/* JADX INFO: loaded from: classes.dex */
public class LimitInputFilter extends LoginFilter.UsernameFilterGeneric {
    private static final String TAG = "LimitInputFilter";
    private String mAllowedDigits;
    public OnClickEnterLister onClickEnterLister;

    public interface OnClickEnterLister {
        void click();
    }

    public LimitInputFilter(String str) {
        this.mAllowedDigits = str;
    }

    @Override // android.text.LoginFilter.UsernameFilterGeneric, android.text.LoginFilter
    public boolean isAllowed(char c) {
        String strValueOf = String.valueOf(c);
        Log.e(TAG, "isAllowed: " + strValueOf);
        if (this.mAllowedDigits.indexOf(c) != -1) {
            return true;
        }
        if (strValueOf.contains("\n")) {
            Log.e(TAG, "有换行");
            OnClickEnterLister onClickEnterLister = this.onClickEnterLister;
            if (onClickEnterLister != null) {
                onClickEnterLister.click();
            }
        }
        return false;
    }

    public void setOnClickEnterLister(OnClickEnterLister onClickEnterLister) {
        this.onClickEnterLister = onClickEnterLister;
    }
}