package cn.com.heaton.shiningmask.ui.widget.loopviewpager;

import android.content.Context;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import androidx.viewpager.widget.ViewPager;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import com.cdbwsoft.library.audio.AudioPlayer;

/* JADX INFO: loaded from: classes.dex */
public class ViewPagerAllResponse extends ViewPager {
    private static final float DISTANCE = 0.0f;
    private int currentIndex;
    private float downX;
    private float downY;
    private boolean isSlide;
    private int mMaxNumber;
    private int startCurrentIndex;
    private float upX;
    private float upY;

    private void initData() {
    }

    public ViewPagerAllResponse(Context context) {
        this(context, null);
        initData();
    }

    public ViewPagerAllResponse(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.currentIndex = 0;
        this.startCurrentIndex = AudioPlayer.HEADER_SAMPLE_RATE;
        this.isSlide = false;
        initData();
    }

    @Override // android.view.ViewGroup, android.view.View
    public boolean dispatchTouchEvent(MotionEvent motionEvent) {
        if (motionEvent.getAction() == 0) {
            this.downX = motionEvent.getX();
            this.downY = motionEvent.getY();
        } else if (motionEvent.getAction() == 1) {
            this.upX = motionEvent.getX();
            this.upY = motionEvent.getY();
            if (Math.abs(this.upX - this.downX) > 0.0f || Math.abs(this.upY - this.downY) > 0.0f) {
                return super.dispatchTouchEvent(motionEvent);
            }
            if (ClickFilter.filter()) {
                return false;
            }
            View viewClickPageOnScreen = clickPageOnScreen(motionEvent);
            if (viewClickPageOnScreen != null) {
                int iIntValue = ((Integer) viewClickPageOnScreen.getTag()).intValue();
                if (getCurrentItem() != iIntValue) {
                    setCurrentItem(iIntValue);
                }
            }
            return true;
        }
        return super.dispatchTouchEvent(motionEvent);
    }

    private View clickPageOnScreen(MotionEvent motionEvent) {
        int childCount = getChildCount();
        int currentItem = getCurrentItem();
        int[] iArr = new int[2];
        float rawX = motionEvent.getRawX();
        for (int i = 0; i < childCount; i++) {
            View childAt = getChildAt(i);
            int iIntValue = ((Integer) childAt.getTag()).intValue();
            childAt.getLocationOnScreen(iArr);
            int width = iArr[0];
            int width2 = childAt.getWidth() + width;
            if (iIntValue < currentItem) {
                width2 = (int) (((double) width2) - (((double) (childAt.getWidth() * 0.3f)) * 0.5d));
                width = (int) (((double) width) - (((double) (childAt.getWidth() * 0.3f)) * 0.5d));
            }
            if (rawX > width && rawX < width2) {
                return childAt;
            }
        }
        return null;
    }

    @Override // androidx.viewpager.widget.ViewPager, android.view.ViewGroup
    public boolean onInterceptTouchEvent(MotionEvent motionEvent) {
        int action = motionEvent.getAction();
        if (action == 0) {
            this.downY = motionEvent.getY();
        } else if (action == 1) {
            float y = motionEvent.getY();
            this.upY = y;
            if (Math.abs(y - this.downY) > 0.0f) {
                return true;
            }
        }
        return super.onInterceptTouchEvent(motionEvent);
    }

    @Override // androidx.viewpager.widget.ViewPager, android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        int action = motionEvent.getAction();
        if (action == 0) {
            this.downY = motionEvent.getY();
        } else if (action == 1) {
            float y = motionEvent.getY();
            this.upY = y;
            if (Math.abs(y - this.downY) > 0.0f) {
                return super.onTouchEvent(motionEvent);
            }
        }
        return super.onTouchEvent(motionEvent);
    }
}