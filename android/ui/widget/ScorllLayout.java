package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewConfiguration;
import android.view.ViewGroup;
import android.widget.Scroller;
import androidx.core.view.ViewConfigurationCompat;

/* JADX INFO: loaded from: classes.dex */
public class ScorllLayout extends ViewGroup {
    private int leftBorder;
    private Scroller mScroller;
    private int mTouchSlop;
    private float mXDown;
    private float mXLastMove;
    private float mXMove;
    private int rightBorder;

    @Override // android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        return false;
    }

    public ScorllLayout(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mScroller = new Scroller(context);
        this.mTouchSlop = ViewConfigurationCompat.getScaledPagingTouchSlop(ViewConfiguration.get(context));
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        super.onMeasure(i, i2);
        int childCount = getChildCount();
        for (int i3 = 0; i3 < childCount; i3++) {
            measureChild(getChildAt(i3), i, i2);
        }
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        if (z) {
            int childCount = getChildCount();
            int i5 = 0;
            while (i5 < childCount) {
                View childAt = getChildAt(i5);
                int measuredWidth = childAt.getMeasuredWidth() * i5;
                i5++;
                childAt.layout(measuredWidth, 0, childAt.getMeasuredWidth() * i5, childAt.getMeasuredHeight());
            }
            this.leftBorder = getChildAt(0).getLeft();
            this.rightBorder = getChildAt(getChildCount() - 1).getRight();
        }
    }
}