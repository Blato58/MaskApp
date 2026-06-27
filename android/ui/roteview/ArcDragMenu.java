package cn.com.heaton.shiningmask.ui.roteview;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Path;
import android.graphics.RectF;
import android.graphics.Region;
import android.util.AttributeSet;
import android.util.TypedValue;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public class ArcDragMenu extends ViewGroup {
    private static final double FLINGABLE_VALUE = 0.05235987755982988d;
    private static final double NOCLICK_VALUE = 0.03490658503988659d;
    double angleDelay;
    int childHeight;
    private boolean isFling;
    private double mCurrAngle;
    private int mDirection;
    private long mDownTime;
    private AutoFlingRunnable mFlingRunnable;
    private double mInitialAngle;
    private int[] mItemImgs;
    private float mLastX;
    private float mLastY;
    private OnMenuItemClickListener mMenuItemClickListener;
    private int mMenuItemCount;
    private int mMenuItemLayoutId;
    Path mPath;
    private int mRadius;
    private float mTmpAngle;
    private int mVisiableItemCount;
    private int mYoffset;
    Region re;

    private enum Direction {
        BOTTOM,
        TOP
    }

    public interface OnMenuItemClickListener {
        void onItemClick(View view, int i);
    }

    public void setOnMenuItemClickListener(OnMenuItemClickListener onMenuItemClickListener) {
        this.mMenuItemClickListener = onMenuItemClickListener;
    }

    public ArcDragMenu(Context context) {
        this(context, null);
    }

    public ArcDragMenu(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public ArcDragMenu(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mInitialAngle = 0.0d;
        this.mCurrAngle = 0.0d;
        this.childHeight = 0;
        this.mMenuItemLayoutId = R.layout.item_arcdragmenu;
        this.re = new Region();
        this.mPath = new Path();
        TypedArray typedArrayObtainStyledAttributes = context.getTheme().obtainStyledAttributes(attributeSet, R.styleable.ArcDragMenu, i, 0);
        this.mRadius = (int) typedArrayObtainStyledAttributes.getDimension(R.styleable.ArcDragMenu_mradius, TypedValue.applyDimension(1, 360.0f, getResources().getDisplayMetrics()));
        this.mVisiableItemCount = typedArrayObtainStyledAttributes.getInteger(R.styleable.ArcDragMenu_visibleitemcount, 5);
        this.mDirection = typedArrayObtainStyledAttributes.getInteger(R.styleable.ArcDragMenu_direction, 0);
        this.mYoffset = (int) typedArrayObtainStyledAttributes.getDimension(R.styleable.ArcDragMenu_yoffset, TypedValue.applyDimension(1, 0.0f, getResources().getDisplayMetrics()));
        typedArrayObtainStyledAttributes.recycle();
        setWillNotDraw(false);
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        int childCount = getChildCount();
        for (int i3 = 0; i3 < childCount; i3++) {
            measureChild(getChildAt(i3), i, i2);
        }
        super.onMeasure(i, i2);
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        int iCos;
        int i5;
        double dAsin = Math.asin((((double) getMeasuredWidth()) / 2.0d) / ((double) this.mRadius)) * 2.0d;
        int i6 = this.mVisiableItemCount;
        double d = dAsin / ((double) i6);
        this.angleDelay = d;
        double d2 = d * (-((((double) i6) / 2.0d) - 0.5d));
        this.mInitialAngle = d2;
        if (this.mCurrAngle == 0.0d) {
            this.mCurrAngle = d2;
        }
        double d3 = this.mCurrAngle;
        int childCount = getChildCount();
        for (int i7 = 0; i7 < childCount; i7++) {
            View childAt = getChildAt(i7);
            this.childHeight = childAt.getMeasuredHeight();
            int iSin = (((int) (((double) this.mRadius) * Math.sin(d3))) + (getMeasuredWidth() / 2)) - (childAt.getMeasuredWidth() / 2);
            if (this.mDirection == Direction.TOP.ordinal()) {
                int i8 = this.mRadius;
                iCos = (int) (((double) i8) - (((double) i8) * Math.cos(d3)));
                i5 = this.mYoffset;
            } else {
                iCos = (int) (((double) this.mRadius) * Math.cos(d3));
                i5 = this.mYoffset;
            }
            int i9 = iCos + i5;
            childAt.layout(iSin, i9, childAt.getMeasuredWidth() + iSin, childAt.getMeasuredHeight() + i9);
            d3 += this.angleDelay;
        }
    }

    @Override // android.view.ViewGroup, android.view.View
    public boolean dispatchTouchEvent(MotionEvent motionEvent) {
        float f;
        if (!isInViewRect(motionEvent.getX(), motionEvent.getY())) {
            return false;
        }
        float rawX = motionEvent.getRawX();
        float rawY = motionEvent.getRawY();
        int action = motionEvent.getAction();
        if (action == 0) {
            this.mLastX = rawX;
            this.mLastY = rawY;
            this.mDownTime = System.currentTimeMillis();
            this.mTmpAngle = 0.0f;
            if (this.isFling) {
                removeCallbacks(this.mFlingRunnable);
                this.isFling = false;
                return true;
            }
            if (!isInChildView((int) rawX, (int) rawY)) {
                return true;
            }
        } else if (action == 1) {
            float fCurrentTimeMillis = (this.mTmpAngle * 1000.0f) / (System.currentTimeMillis() - this.mDownTime);
            if (Math.abs(fCurrentTimeMillis) > FLINGABLE_VALUE && !this.isFling) {
                AutoFlingRunnable autoFlingRunnable = new AutoFlingRunnable(fCurrentTimeMillis);
                this.mFlingRunnable = autoFlingRunnable;
                post(autoFlingRunnable);
                return true;
            }
            if (Math.abs(this.mTmpAngle) > NOCLICK_VALUE || System.currentTimeMillis() - this.mDownTime > 500) {
                return true;
            }
        } else if (action == 2) {
            float angle = getAngle(rawX, rawY) - getAngle(this.mLastX, this.mLastY);
            double d = this.mCurrAngle;
            double d2 = angle;
            double d3 = d + d2;
            double d4 = this.mInitialAngle;
            if (d3 <= d4) {
                f = angle;
                if (d + d2 >= d4 - (((double) (this.mMenuItemCount - this.mVisiableItemCount)) * this.angleDelay)) {
                    this.mCurrAngle = d + d2;
                }
            } else {
                f = angle;
            }
            this.mTmpAngle += f;
            requestLayout();
            this.mLastX = rawX;
            this.mLastY = rawY;
        }
        return super.dispatchTouchEvent(motionEvent);
    }

    private float getAngle(float f, float f2) {
        double measuredWidth = f - (getMeasuredWidth() / 2);
        return (float) Math.asin(measuredWidth / Math.hypot(measuredWidth, f2));
    }

    public void setMenuItemIcons(int[] iArr) {
        this.mItemImgs = iArr;
        if (iArr == null) {
            throw new IllegalArgumentException("菜单项至少设置一项");
        }
        if (iArr != null) {
            this.mMenuItemCount = iArr.length;
        }
        addMenuItems();
    }

    public void setMenuItemLayoutId(int i) {
        this.mMenuItemLayoutId = i;
    }

    private void addMenuItems() {
        LayoutInflater layoutInflaterFrom = LayoutInflater.from(getContext());
        for (final int i = 0; i < this.mMenuItemCount; i++) {
            View viewInflate = layoutInflaterFrom.inflate(this.mMenuItemLayoutId, (ViewGroup) this, false);
            ImageView imageView = (ImageView) viewInflate.findViewById(R.id.iv_item);
            if (imageView != null) {
                imageView.setVisibility(0);
                imageView.setImageResource(this.mItemImgs[i]);
                imageView.setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.roteview.ArcDragMenu.1
                    @Override // android.view.View.OnClickListener
                    public void onClick(View view) {
                        if (ArcDragMenu.this.mMenuItemClickListener != null) {
                            ArcDragMenu.this.mMenuItemClickListener.onItemClick(view, i);
                        }
                    }
                });
            }
            addView(viewInflate);
        }
    }

    private class AutoFlingRunnable implements Runnable {
        private float angelPerSecond;

        public AutoFlingRunnable(float f) {
            this.angelPerSecond = f;
        }

        @Override // java.lang.Runnable
        public void run() {
            if (Math.abs(this.angelPerSecond) < 0.1f) {
                ArcDragMenu.this.isFling = false;
                return;
            }
            ArcDragMenu.this.isFling = true;
            double d = this.angelPerSecond / 60.0f;
            if (ArcDragMenu.this.mCurrAngle + d <= ArcDragMenu.this.mInitialAngle && ArcDragMenu.this.mCurrAngle + d >= ArcDragMenu.this.mInitialAngle - (((double) (ArcDragMenu.this.mMenuItemCount - ArcDragMenu.this.mVisiableItemCount)) * ArcDragMenu.this.angleDelay)) {
                ArcDragMenu.this.mCurrAngle += d;
            } else if (ArcDragMenu.this.mCurrAngle + d <= ArcDragMenu.this.mInitialAngle) {
                ArcDragMenu arcDragMenu = ArcDragMenu.this;
                arcDragMenu.mCurrAngle = arcDragMenu.mInitialAngle - (((double) (ArcDragMenu.this.mMenuItemCount - ArcDragMenu.this.mVisiableItemCount)) * ArcDragMenu.this.angleDelay);
            } else if (ArcDragMenu.this.mCurrAngle + d >= ArcDragMenu.this.mInitialAngle - (((double) (ArcDragMenu.this.mMenuItemCount - ArcDragMenu.this.mVisiableItemCount)) * ArcDragMenu.this.angleDelay)) {
                ArcDragMenu arcDragMenu2 = ArcDragMenu.this;
                arcDragMenu2.mCurrAngle = arcDragMenu2.mInitialAngle;
            }
            this.angelPerSecond /= 1.066f;
            ArcDragMenu.this.postDelayed(this, 10L);
            ArcDragMenu.this.requestLayout();
        }
    }

    public boolean isInViewRect(float f, float f2) {
        int i;
        int i2;
        float f3;
        int i3;
        int i4;
        int i5 = this.mRadius;
        if (this.mDirection == Direction.TOP.ordinal()) {
            int i6 = this.childHeight;
            int i7 = this.mYoffset;
            i = i6 + i7;
            i2 = (i5 * 2) + i6 + i7;
            f3 = ((float) (90.0d - (((this.angleDelay * ((double) (this.mVisiableItemCount / 2.0f))) * 180.0d) / 3.141592653589793d))) + 180.0f;
        } else {
            int i8 = this.mYoffset;
            i = (-i5) + i8;
            i2 = i5 + i8;
            f3 = (float) (90.0d - (((this.angleDelay * ((double) (this.mVisiableItemCount / 2.0f))) * 180.0d) / 3.141592653589793d));
        }
        float f4 = (float) (((this.angleDelay * 5.0d) * 180.0d) / 3.141592653589793d);
        this.mPath.arcTo(new RectF((getMeasuredWidth() / 2) - i5, i, (getMeasuredWidth() / 2) + i5, i2), f3, f4);
        int i9 = this.mRadius + this.childHeight;
        if (this.mDirection == Direction.TOP.ordinal()) {
            i3 = this.mYoffset;
            i4 = (i9 * 2) + i3;
        } else {
            int i10 = this.mYoffset;
            i3 = (-i9) + i10;
            i4 = i10 + i9;
        }
        this.mPath.arcTo(new RectF((getMeasuredWidth() / 2) - i9, i3, (getMeasuredWidth() / 2) + i9, i4), f3 + f4, -f4);
        this.mPath.close();
        RectF rectF = new RectF();
        this.mPath.computeBounds(rectF, true);
        this.re.setPath(this.mPath, new Region((int) rectF.left, (int) rectF.top, (int) rectF.right, (int) rectF.bottom));
        return this.re.contains((int) f, (int) f2);
    }

    public boolean isInChildView(int i, int i2) {
        int childCount = getChildCount();
        for (int i3 = 0; i3 < childCount; i3++) {
            if (isTouchPointInView(getChildAt(i3), i, i2)) {
                return true;
            }
        }
        return false;
    }

    private boolean isTouchPointInView(View view, int i, int i2) {
        if (view == null) {
            return false;
        }
        int[] iArr = new int[2];
        view.getLocationOnScreen(iArr);
        int i3 = iArr[0];
        int i4 = iArr[1];
        return i2 >= i4 && i2 <= view.getMeasuredHeight() + i4 && i >= i3 && i <= view.getMeasuredWidth() + i3;
    }
}