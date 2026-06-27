package cn.com.heaton.shiningmask.ui.utils;

import android.R;
import android.app.Activity;
import android.util.Log;
import android.view.View;
import android.view.ViewTreeObserver;

/* JADX INFO: loaded from: classes.dex */
public class KeyboardChangeListener implements ViewTreeObserver.OnGlobalLayoutListener {
    private static final String TAG = "ListenerHandler";
    private View mContentView;
    private KeyBoardListener mKeyBoardListen;
    private int mOriginHeight;
    private int mPreHeight;

    public interface KeyBoardListener {
        void onKeyboardChange(boolean z, int i);
    }

    public void setKeyBoardListener(KeyBoardListener keyBoardListener) {
        this.mKeyBoardListen = keyBoardListener;
    }

    public KeyboardChangeListener(Activity activity) {
        if (activity == null) {
            Log.i(TAG, "contextObj is null");
            return;
        }
        View viewFindContentView = findContentView(activity);
        this.mContentView = viewFindContentView;
        if (viewFindContentView != null) {
            addContentTreeObserver();
        }
    }

    private View findContentView(Activity activity) {
        return activity.findViewById(R.id.content);
    }

    private void addContentTreeObserver() {
        this.mContentView.getViewTreeObserver().addOnGlobalLayoutListener(this);
    }

    @Override // android.view.ViewTreeObserver.OnGlobalLayoutListener
    public void onGlobalLayout() {
        int i;
        boolean z;
        int height = this.mContentView.getHeight();
        if (height == 0) {
            Log.i(TAG, "currHeight is 0");
            return;
        }
        int i2 = this.mPreHeight;
        if (i2 == 0) {
            this.mPreHeight = height;
            this.mOriginHeight = height;
            return;
        }
        if (i2 != height) {
            this.mPreHeight = height;
            int i3 = this.mOriginHeight;
            if (i3 == height) {
                z = false;
                i = 0;
            } else {
                i = i3 - height;
                z = true;
            }
            KeyBoardListener keyBoardListener = this.mKeyBoardListen;
            if (keyBoardListener != null) {
                keyBoardListener.onKeyboardChange(z, i);
            }
        }
    }

    public void destroy() {
        if (this.mContentView != null) {
            this.mContentView.getViewTreeObserver().removeOnGlobalLayoutListener(this);
        }
    }
}