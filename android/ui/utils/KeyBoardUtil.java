package cn.com.heaton.shiningmask.ui.utils;

import android.app.Activity;
import android.content.Context;
import android.graphics.Rect;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Toast;

/* JADX INFO: loaded from: classes.dex */
public class KeyBoardUtil {
    public static void showKeyBoard(Context context, View view) {
        ((InputMethodManager) context.getSystemService("input_method")).showSoftInput(view, 2);
    }

    public static void hideKeyBoard(Activity activity) {
        ((InputMethodManager) activity.getSystemService("input_method")).hideSoftInputFromWindow(activity.getWindow().getDecorView().getWindowToken(), 0);
    }

    private void addKeyBoardListener(final Activity activity) {
        activity.getWindow().getDecorView().addOnLayoutChangeListener(new View.OnLayoutChangeListener() { // from class: cn.com.heaton.shiningmask.ui.utils.KeyBoardUtil.1
            @Override // android.view.View.OnLayoutChangeListener
            public void onLayoutChange(View view, int i, int i2, int i3, int i4, int i5, int i6, int i7, int i8) {
                Rect rect = new Rect();
                activity.getWindow().getDecorView().getWindowVisibleDisplayFrame(rect);
                if (i4 != 0 && i8 != 0 && i4 - rect.bottom <= 0) {
                    Toast.makeText(activity, "隐藏", 0).show();
                } else {
                    Toast.makeText(activity, "弹出", 0).show();
                }
            }
        });
    }
}