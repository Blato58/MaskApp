package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.widget.ScrollView;

/* JADX INFO: loaded from: classes.dex */
public class MyScrollView extends ScrollView {
    private boolean scroll;

    public MyScrollView(Context context) {
        super(context);
        this.scroll = true;
    }

    public MyScrollView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.scroll = true;
    }

    public MyScrollView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.scroll = true;
    }

    public void setScroll(boolean z) {
        this.scroll = z;
    }

    @Override // android.widget.ScrollView, android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        if (this.scroll) {
            return super.onTouchEvent(motionEvent);
        }
        return false;
    }
}