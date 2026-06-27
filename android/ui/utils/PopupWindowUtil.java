package cn.com.heaton.shiningmask.ui.utils;

import android.util.Log;
import android.view.View;

/* JADX INFO: loaded from: classes.dex */
public class PopupWindowUtil {
    private static final String TAG = "PopupWindowUtil";

    public static int[] calculatePopWindowPos(View view, View view2) {
        int[] iArr = new int[2];
        view.getLocationOnScreen(iArr);
        int height = view.getHeight();
        int screenHeight = ScreenUtils.getScreenHeight(view.getContext());
        int screenWidth = ScreenUtils.getScreenWidth(view.getContext());
        view2.measure(0, 0);
        int measuredHeight = view2.getMeasuredHeight();
        int measuredWidth = view2.getMeasuredWidth();
        view.measure(0, 0);
        int measuredHeight2 = view.getMeasuredHeight();
        view.getMeasuredWidth();
        Log.e(TAG, "calculatePopWindowPos: isNeedShowUp:" + ((screenHeight - iArr[1]) - height < measuredHeight));
        int[] iArr2 = {screenWidth - measuredWidth, iArr[1] + ((measuredHeight2 / 2) - (measuredHeight / 2))};
        Log.e(TAG, "calculatePopWindowPos windowHeight: " + measuredHeight + "  windowHeight1:" + measuredHeight2);
        return iArr2;
    }
}