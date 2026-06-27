package cn.com.heaton.shiningmask.ui.widget.image3d1;

import android.content.Context;
import android.os.Handler;
import android.util.AttributeSet;
import android.util.Log;
import android.view.MotionEvent;
import android.view.VelocityTracker;
import android.view.View;
import android.view.ViewConfiguration;
import android.view.ViewGroup;
import android.widget.Scroller;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;

/* JADX INFO: loaded from: classes.dex */
public class Image3DSwitchView extends ViewGroup {
    public static final int IMAGE_PADDING = 15;
    private static final int SCROLL_BACK = 2;
    private static final int SCROLL_NEXT = 0;
    private static final int SCROLL_PREVIOUS = 1;
    private static final int SNAP_VELOCITY = 0;
    private static final String TAG = "Image3DSwitch";
    private static final int TOUCH_STATE_REST = 0;
    private static final int TOUCH_STATE_SCROLLING = 1;
    private static Handler handler = new Handler();
    public static int mWidth;
    private boolean forceToRelayout;
    private OnImageItemClickListener imageItemClickListener;
    private int mCount;
    private int mCurrentImage;
    private int mHeight;
    private int mImageWidth;
    private int[] mItems;
    private float mLastMotionX;
    private OnImageSwitchListener mListener;
    private Scroller mScroller;
    private int mTouchSlop;
    private int mTouchState;
    private VelocityTracker mVelocityTracker;

    public interface OnImageItemClickListener {
        void onClick(int i);

        void onTouch();

        void onTouchStop();
    }

    public interface OnImageSwitchListener {
        void onImageSwitch(int i);
    }

    public Image3DSwitchView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mTouchState = 0;
        this.mTouchSlop = ViewConfiguration.get(getContext()).getScaledTouchSlop();
        LogUtil.d("mTouchSlop:" + this.mTouchSlop);
        this.mScroller = new Scroller(context);
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        if (z || this.forceToRelayout) {
            this.mCount = getChildCount();
            Log.e(TAG, "图片数量:" + this.mCount);
            if (this.mCount < 4) {
                return;
            }
            mWidth = getMeasuredWidth();
            this.mHeight = getMeasuredHeight();
            this.mImageWidth = (int) (((double) mWidth) * 0.62d);
            LogUtil.d("mImageWidth:" + this.mImageWidth);
            int i5 = this.mCurrentImage;
            if (i5 >= 0 && i5 < this.mCount) {
                this.mScroller.abortAnimation();
                setScrollX(0);
                int i6 = this.mImageWidth;
                int i7 = ((-i6) * 2) + ((mWidth - i6) / 2);
                int[] iArr = {getIndexForItem(0), getIndexForItem(1), getIndexForItem(2), getIndexForItem(3), getIndexForItem(4), getIndexForItem(5), getIndexForItem(6), getIndexForItem(7)};
                this.mItems = iArr;
                for (int i8 = 0; i8 < 8; i8++) {
                    Log.e(TAG, "items:" + iArr[i8]);
                    final Image3DView image3DView = (Image3DView) getChildAt(iArr[i8]);
                    image3DView.layout(i7 + 15, 0, (this.mImageWidth + i7) - 15, this.mHeight);
                    image3DView.initImageViewBitmap();
                    i7 += this.mImageWidth;
                    image3DView.setImageNo(i8);
                    image3DView.setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.widget.image3d1.Image3DSwitchView.1
                        @Override // android.view.View.OnClickListener
                        public void onClick(View view) {
                            Log.e(Image3DSwitchView.TAG, "====Click" + image3DView.getImageNo() + "   index:" + image3DView.getmIndex());
                            if (Image3DSwitchView.this.imageItemClickListener != null) {
                                if (image3DView.getmIndex() == 1) {
                                    Image3DSwitchView.this.imageItemClickListener.onTouch();
                                    Image3DSwitchView.this.scrollToPrevious();
                                } else if (image3DView.getmIndex() == 2) {
                                    Image3DSwitchView.this.imageItemClickListener.onClick(Image3DSwitchView.this.mCurrentImage);
                                } else if (image3DView.getmIndex() == 3) {
                                    Image3DSwitchView.this.imageItemClickListener.onTouch();
                                    Image3DSwitchView.this.scrollToNext();
                                }
                            }
                        }
                    });
                }
                refreshImageShowing();
            }
            this.forceToRelayout = false;
        }
    }

    @Override // android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        if (this.mScroller.isFinished()) {
            if (this.mVelocityTracker == null) {
                this.mVelocityTracker = VelocityTracker.obtain();
            }
            this.mVelocityTracker.addMovement(motionEvent);
            int action = motionEvent.getAction();
            float x = motionEvent.getX();
            if (action == 0) {
                Log.e(TAG, "ACTION_DOWN");
                this.mLastMotionX = x;
            } else if (action == 1) {
                Log.e(TAG, "ACTION_UP");
                this.mVelocityTracker.computeCurrentVelocity(1000);
                int xVelocity = (int) this.mVelocityTracker.getXVelocity();
                if (shouldScrollToNext(xVelocity)) {
                    scrollToNext();
                } else if (shouldScrollToPrevious(xVelocity)) {
                    scrollToPrevious();
                } else {
                    scrollBack();
                }
                VelocityTracker velocityTracker = this.mVelocityTracker;
                if (velocityTracker != null) {
                    velocityTracker.recycle();
                    this.mVelocityTracker = null;
                }
            } else if (action == 2) {
                Log.e(TAG, "ACTION_MOVE");
                int i = (int) (this.mLastMotionX - x);
                this.mLastMotionX = x;
                scrollBy(i, 0);
                refreshImageShowing();
                OnImageItemClickListener onImageItemClickListener = this.imageItemClickListener;
                if (onImageItemClickListener != null) {
                    onImageItemClickListener.onTouch();
                }
            }
        }
        return true;
    }

    @Override // android.view.ViewGroup
    public boolean onInterceptTouchEvent(MotionEvent motionEvent) {
        int action = motionEvent.getAction();
        if (action == 2 && this.mTouchState != 0) {
            return true;
        }
        float x = motionEvent.getX();
        if (action == 0) {
            this.mLastMotionX = x;
            this.mTouchState = 0;
        } else if (action == 2) {
            if (((int) Math.abs(this.mLastMotionX - x)) > this.mTouchSlop) {
                this.mTouchState = 1;
            }
        } else {
            this.mTouchState = 0;
        }
        return this.mTouchState != 0;
    }

    @Override // android.view.View
    public void computeScroll() {
        if (this.mScroller.computeScrollOffset()) {
            scrollTo(this.mScroller.getCurrX(), this.mScroller.getCurrY());
            refreshImageShowing();
            postInvalidate();
        }
    }

    public void setOnImageSwitchListener(OnImageSwitchListener onImageSwitchListener) {
        this.mListener = onImageSwitchListener;
    }

    public void setOnImageItemClickListener(OnImageItemClickListener onImageItemClickListener) {
        this.imageItemClickListener = onImageItemClickListener;
    }

    public void setCurrentImage(int i) {
        this.mCurrentImage = i;
        requestLayout();
    }

    public void scrollToNext() {
        if (this.mScroller.isFinished()) {
            int scrollX = this.mImageWidth - getScrollX();
            checkImageSwitchBorder(0);
            OnImageSwitchListener onImageSwitchListener = this.mListener;
            if (onImageSwitchListener != null) {
                onImageSwitchListener.onImageSwitch(this.mCurrentImage);
            }
            beginScroll(getScrollX(), 0, scrollX, 0, 0);
        }
    }

    public void scrollToPrevious() {
        if (this.mScroller.isFinished()) {
            Log.e(TAG, "滚动到上一张图片");
            int scrollX = (-this.mImageWidth) - getScrollX();
            checkImageSwitchBorder(1);
            OnImageSwitchListener onImageSwitchListener = this.mListener;
            if (onImageSwitchListener != null) {
                onImageSwitchListener.onImageSwitch(this.mCurrentImage);
            }
            beginScroll(getScrollX(), 0, scrollX, 0, 1);
        }
    }

    public void scrollBack() {
        if (this.mScroller.isFinished()) {
            Log.e(TAG, "滚动回原图片");
            beginScroll(getScrollX(), 0, -getScrollX(), 0, 2);
            OnImageItemClickListener onImageItemClickListener = this.imageItemClickListener;
            if (onImageItemClickListener != null) {
                onImageItemClickListener.onTouchStop();
            }
        }
    }

    public void clear() {
        for (int i = 0; i < this.mCount; i++) {
            ((Image3DView) getChildAt(i)).recycleBitmap();
        }
    }

    private void beginScroll(int i, int i2, int i3, int i4, final int i5) {
        int iAbs = (800 / this.mImageWidth) * Math.abs(i3);
        LogUtil.d("duration:" + iAbs);
        this.mScroller.startScroll(i, i2, i3, i4, iAbs);
        invalidate();
        handler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.widget.image3d1.Image3DSwitchView.2
            @Override // java.lang.Runnable
            public void run() {
                int i6 = i5;
                if (i6 == 0 || i6 == 1) {
                    Image3DSwitchView.this.forceToRelayout = true;
                    Image3DSwitchView.this.requestLayout();
                }
            }
        }, iAbs);
    }

    private int getIndexForItem(int i) {
        int i2 = (this.mCurrentImage + i) - 3;
        while (i2 < 0) {
            i2 += this.mCount;
        }
        while (true) {
            int i3 = this.mCount;
            if (i2 <= i3 - 1) {
                return i2;
            }
            i2 -= i3;
        }
    }

    private void refreshImageShowing() {
        int i = 0;
        while (true) {
            int[] iArr = this.mItems;
            if (i >= iArr.length) {
                return;
            }
            Image3DView image3DView = (Image3DView) getChildAt(iArr[i]);
            image3DView.setRotateData(i, getScrollX());
            image3DView.invalidate();
            i++;
        }
    }

    private void checkImageSwitchBorder(int i) {
        if (i == 0) {
            int i2 = this.mCurrentImage + 1;
            this.mCurrentImage = i2;
            if (i2 >= this.mCount) {
                this.mCurrentImage = 0;
                return;
            }
        }
        if (i == 1) {
            int i3 = this.mCurrentImage - 1;
            this.mCurrentImage = i3;
            if (i3 < 0) {
                this.mCurrentImage = this.mCount - 1;
            }
        }
    }

    private boolean shouldScrollToNext(int i) {
        return i < 0 || getScrollX() > this.mImageWidth / 2;
    }

    private boolean shouldScrollToPrevious(int i) {
        return i > 0 || getScrollX() < (-this.mImageWidth) / 2;
    }
}