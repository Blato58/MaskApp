package cn.com.heaton.shiningmask.ui.widget.holocolorpicker;

import android.content.Context;
import android.content.res.Resources;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.RectF;
import android.graphics.SweepGradient;
import android.os.Bundle;
import android.os.Parcelable;
import android.util.AttributeSet;
import android.view.View;
import android.view.WindowManager;
import androidx.core.internal.view.SupportMenu;
import androidx.core.view.InputDeviceCompat;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;

/* JADX INFO: loaded from: classes.dex */
public class ColorPicker extends View {
    private static final int[] COLORS = {SupportMenu.CATEGORY_MASK, -65281, -16776961, -16711681, -16711936, -16711936, -1, InputDeviceCompat.SOURCE_ANY, SupportMenu.CATEGORY_MASK};
    private static final String STATE_ANGLE = "angle";
    private static final String STATE_OLD_COLOR = "color";
    private static final String STATE_PARENT = "parent";
    private static final String STATE_SHOW_OLD_COLOR = "showColor";
    float an;
    private boolean enabled;
    private float mAngle;
    private Paint mCenterHaloPaint;
    private int mCenterNewColor;
    private Paint mCenterNewPaint;
    private int mCenterOldColor;
    private Paint mCenterOldPaint;
    private RectF mCenterRectangle;
    private int mColor;
    private int mColorCenterHaloRadius;
    private int mColorCenterRadius;
    private int mColorPointerHaloRadius;
    private int mColorPointerRadius;
    private Paint mColorWheelPaint;
    private int mColorWheelRadius;
    private RectF mColorWheelRectangle;
    private int mColorWheelThickness;
    private float[] mHSV;
    private Paint mPointerColor;
    private Paint mPointerHaloPaint;
    private int mPreferredColorCenterHaloRadius;
    private int mPreferredColorCenterRadius;
    private int mPreferredColorWheelRadius;
    private SVBar mSVbar;
    private SaturationBar mSaturationBar;
    private boolean mShowCenterOldColor;
    private float mSlopX;
    private float mSlopY;
    private boolean mTouchAnywhereOnColorWheelEnabled;
    private float mTranslationOffset;
    private boolean mUserIsMovingPointer;
    private ValueBar mValueBar;
    private int oldChangedListenerColor;
    private int oldSelectedListenerColor;
    private OnColorChangedListener onColorChangedListener;
    private OnColorSelectedListener onColorSelectedListener;

    public interface OnColorChangedListener {
        void onAlphaChanged(int i);

        void onColorChanged(int i, float f);
    }

    public interface OnColorSelectedListener {
        void onColorSelected(int i);

        void onStartSelected(int i);
    }

    public void addOpacityBar(OpacityBar opacityBar) {
    }

    public ColorPicker(Context context) {
        super(context);
        this.mColorWheelRectangle = new RectF();
        this.mCenterRectangle = new RectF();
        this.mUserIsMovingPointer = false;
        this.mHSV = new float[3];
        this.mSVbar = null;
        this.mSaturationBar = null;
        this.mTouchAnywhereOnColorWheelEnabled = true;
        this.mValueBar = null;
        this.enabled = true;
        init(null, 0);
    }

    public ColorPicker(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mColorWheelRectangle = new RectF();
        this.mCenterRectangle = new RectF();
        this.mUserIsMovingPointer = false;
        this.mHSV = new float[3];
        this.mSVbar = null;
        this.mSaturationBar = null;
        this.mTouchAnywhereOnColorWheelEnabled = true;
        this.mValueBar = null;
        this.enabled = true;
        init(attributeSet, 0);
    }

    public ColorPicker(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mColorWheelRectangle = new RectF();
        this.mCenterRectangle = new RectF();
        this.mUserIsMovingPointer = false;
        this.mHSV = new float[3];
        this.mSVbar = null;
        this.mSaturationBar = null;
        this.mTouchAnywhereOnColorWheelEnabled = true;
        this.mValueBar = null;
        this.enabled = true;
        init(attributeSet, i);
    }

    public void setOnColorChangedListener(OnColorChangedListener onColorChangedListener) {
        this.onColorChangedListener = onColorChangedListener;
    }

    public OnColorChangedListener getOnColorChangedListener() {
        return this.onColorChangedListener;
    }

    public void setOnColorSelectedListener(OnColorSelectedListener onColorSelectedListener) {
        this.onColorSelectedListener = onColorSelectedListener;
    }

    public OnColorSelectedListener getOnColorSelectedListener() {
        return this.onColorSelectedListener;
    }

    private void init(AttributeSet attributeSet, int i) {
        TypedArray typedArrayObtainStyledAttributes = getContext().obtainStyledAttributes(attributeSet, R.styleable.ColorPicker, i, 0);
        Resources resources = getContext().getResources();
        this.mColorWheelThickness = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorPicker_color_wheel_thickness, resources.getDimensionPixelSize(R.dimen.color_wheel_thickness));
        int dimensionPixelSize = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorPicker_color_wheel_radius, resources.getDimensionPixelSize(R.dimen.color_wheel_radius));
        this.mColorWheelRadius = dimensionPixelSize;
        this.mPreferredColorWheelRadius = dimensionPixelSize;
        int dimensionPixelSize2 = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorPicker_color_center_radius, resources.getDimensionPixelSize(R.dimen.color_center_radius));
        this.mColorCenterRadius = dimensionPixelSize2;
        this.mPreferredColorCenterRadius = dimensionPixelSize2;
        int dimensionPixelSize3 = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorPicker_color_center_halo_radius, resources.getDimensionPixelSize(R.dimen.color_center_halo_radius));
        this.mColorCenterHaloRadius = dimensionPixelSize3;
        this.mPreferredColorCenterHaloRadius = dimensionPixelSize3;
        this.mColorPointerRadius = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorPicker_color_pointer_radius, resources.getDimensionPixelSize(R.dimen.color_pointer_radius));
        this.mColorPointerHaloRadius = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorPicker_color_pointer_halo_radius, resources.getDimensionPixelSize(R.dimen.color_pointer_halo_radius));
        typedArrayObtainStyledAttributes.recycle();
        this.mAngle = -1.5707964f;
        SweepGradient sweepGradient = new SweepGradient(0.0f, 0.0f, COLORS, (float[]) null);
        Paint paint = new Paint(1);
        this.mColorWheelPaint = paint;
        paint.setShader(sweepGradient);
        this.mColorWheelPaint.setStyle(Paint.Style.STROKE);
        this.mColorWheelPaint.setStrokeWidth(this.mColorWheelThickness);
        Paint paint2 = new Paint(1);
        this.mPointerHaloPaint = paint2;
        paint2.setShader(sweepGradient);
        this.mPointerHaloPaint.setStyle(Paint.Style.STROKE);
        this.mPointerHaloPaint.setStrokeWidth(3.0f);
        Paint paint3 = new Paint(1);
        this.mPointerColor = paint3;
        paint3.setColor(calculateColor(this.mAngle));
        Paint paint4 = new Paint(1);
        this.mCenterNewPaint = paint4;
        paint4.setColor(calculateColor(this.mAngle));
        this.mCenterNewPaint.setStyle(Paint.Style.FILL);
        Paint paint5 = new Paint(1);
        this.mCenterOldPaint = paint5;
        paint5.setColor(calculateColor(this.mAngle));
        this.mCenterOldPaint.setStyle(Paint.Style.FILL);
        Paint paint6 = new Paint(1);
        this.mCenterHaloPaint = paint6;
        paint6.setColor(0);
        this.mCenterNewColor = calculateColor(this.mAngle);
        this.mCenterOldColor = calculateColor(this.mAngle);
        this.mShowCenterOldColor = true;
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        float f = this.mTranslationOffset;
        canvas.translate(f, f);
        LogUtil.d("mAngle111：" + this.mAngle);
        calculatePointerPosition(this.mAngle);
        this.mPointerHaloPaint.setAlpha(255);
        this.mPointerHaloPaint.setColor(-1);
        WindowManager windowManager = (WindowManager) getContext().getSystemService("window");
        windowManager.getDefaultDisplay().getWidth();
        windowManager.getDefaultDisplay().getHeight();
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        int iMin = (this.mPreferredColorWheelRadius + this.mColorPointerHaloRadius) * 2;
        int mode = View.MeasureSpec.getMode(i);
        int size = View.MeasureSpec.getSize(i);
        int mode2 = View.MeasureSpec.getMode(i2);
        int size2 = View.MeasureSpec.getSize(i2);
        if (mode != 1073741824) {
            size = mode == Integer.MIN_VALUE ? Math.min(iMin, size) : iMin;
        }
        if (mode2 == 1073741824) {
            iMin = size2;
        } else if (mode2 == Integer.MIN_VALUE) {
            iMin = Math.min(iMin, size2);
        }
        int iMin2 = Math.min(size, iMin);
        setMeasuredDimension(iMin2, iMin2);
        this.mTranslationOffset = iMin2 * 0.5f;
        int i3 = ((iMin2 / 2) - this.mColorWheelThickness) - this.mColorPointerHaloRadius;
        this.mColorWheelRadius = i3;
        this.mColorWheelRectangle.set(-i3, -i3, i3, i3);
        float f = this.mPreferredColorCenterRadius;
        int i4 = this.mColorWheelRadius;
        int i5 = this.mPreferredColorWheelRadius;
        int i6 = (int) (f * (i4 / i5));
        this.mColorCenterRadius = i6;
        this.mColorCenterHaloRadius = (int) (this.mPreferredColorCenterHaloRadius * (i4 / i5));
        this.mCenterRectangle.set(-i6, -i6, i6, i6);
    }

    private int ave(int i, int i2, float f) {
        return i + Math.round(f * (i2 - i));
    }

    private int calculateColor(float f) {
        float f2 = (float) (((double) f) / 6.283185307179586d);
        if (f2 < 0.0f) {
            f2 += 1.0f;
        }
        if (f2 <= 0.0f) {
            int i = COLORS[0];
            this.mColor = i;
            return i;
        }
        if (f2 >= 1.0f) {
            int[] iArr = COLORS;
            this.mColor = iArr[iArr.length - 1];
            return iArr[iArr.length - 1];
        }
        int[] iArr2 = COLORS;
        float length = f2 * (iArr2.length - 1);
        int i2 = (int) length;
        float f3 = length - i2;
        int i3 = iArr2[i2];
        int i4 = iArr2[i2 + 1];
        int iAve = ave(Color.alpha(i3), Color.alpha(i4), f3);
        int iAve2 = ave(Color.red(i3), Color.red(i4), f3);
        int iAve3 = ave(Color.green(i3), Color.green(i4), f3);
        int iAve4 = ave(Color.blue(i3), Color.blue(i4), f3);
        this.mColor = Color.argb(iAve, iAve2, iAve3, iAve4);
        return Color.argb(iAve, iAve2, iAve3, iAve4);
    }

    public int getColor() {
        return this.mCenterNewColor;
    }

    public void setColor(int i) {
        float fColorToAngle = colorToAngle(i);
        this.mAngle = fColorToAngle;
        this.mPointerColor.setColor(calculateColor(fColorToAngle));
        if (this.mSVbar != null) {
            Color.colorToHSV(i, this.mHSV);
            this.mSVbar.setColor(this.mColor);
            float[] fArr = this.mHSV;
            float f = fArr[1];
            float f2 = fArr[2];
            if (f < f2) {
                this.mSVbar.setSaturation(f);
            } else if (f > f2) {
                this.mSVbar.setValue(f2);
            }
        }
        if (this.mSaturationBar != null) {
            Color.colorToHSV(i, this.mHSV);
            this.mSaturationBar.setColor(this.mColor);
            this.mSaturationBar.setSaturation(this.mHSV[1]);
        }
        ValueBar valueBar = this.mValueBar;
        if (valueBar != null && this.mSaturationBar == null) {
            Color.colorToHSV(i, this.mHSV);
            this.mValueBar.setColor(this.mColor);
            this.mValueBar.setValue(this.mHSV[2]);
        } else if (valueBar != null) {
            Color.colorToHSV(i, this.mHSV);
            this.mValueBar.setValue(this.mHSV[2]);
        }
        setNewCenterColor(i);
    }

    private float colorToAngle(int i) {
        Color.colorToHSV(i, new float[3]);
        return (float) Math.toRadians(-r0[0]);
    }

    /* JADX WARN: Removed duplicated region for block: B:53:0x0186  */
    /* JADX WARN: Removed duplicated region for block: B:73:0x01d7  */
    @Override // android.view.View
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct code enable 'Show inconsistent code' option in preferences
    */
    public boolean onTouchEvent(android.view.MotionEvent r10) {
        /*
            Method dump skipped, instruction units count: 485
            To view this dump change 'Code comments level' option to 'DEBUG'
        */
        throw new UnsupportedOperationException("Method not decompiled: cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker.onTouchEvent(android.view.MotionEvent):boolean");
    }

    @Override // android.view.View
    public void setEnabled(boolean z) {
        super.setEnabled(z);
        this.enabled = z;
    }

    public void setStateAngle() {
        setColor(getOldCenterColor());
        invalidate();
    }

    public void setStateAngle(int i) {
        setColor(i);
        invalidate();
    }

    private float[] calculatePointerPosition(float f) {
        double d = f;
        return new float[]{(float) (((double) this.mColorWheelRadius) * Math.cos(d)), (float) (((double) this.mColorWheelRadius) * Math.sin(d))};
    }

    public void addSVBar(SVBar sVBar) {
        this.mSVbar = sVBar;
        sVBar.setColorPicker(this);
        this.mSVbar.setColor(this.mColor);
    }

    public void addSaturationBar(SaturationBar saturationBar) {
        this.mSaturationBar = saturationBar;
        saturationBar.setColorPicker(this);
        this.mSaturationBar.setColor(this.mColor);
    }

    public void addValueBar(ValueBar valueBar) {
        this.mValueBar = valueBar;
        valueBar.setColorPicker(this);
        this.mValueBar.setColor(this.mColor);
    }

    public void setNewCenterColor(int i) {
        if (i == -1 || i == 0) {
            return;
        }
        LogUtil.d("setNewCenterColor:" + i);
        this.mCenterNewColor = i;
        this.mCenterNewPaint.setColor(i);
        if (this.mCenterOldColor == 0) {
            this.mCenterOldColor = i;
            this.mCenterOldPaint.setColor(i);
        }
        invalidate();
    }

    public void setColorAlpha(int i) {
        int i2 = this.mCenterNewColor;
        int i3 = ((i & 255) << 24) | (16777215 & i2);
        this.mCenterNewColor = i3;
        this.mCenterNewPaint.setColor(i3);
        if (this.mCenterOldColor == 0) {
            this.mCenterOldColor = i2;
            this.mCenterOldPaint.setColor(i2);
        }
        OnColorChangedListener onColorChangedListener = this.onColorChangedListener;
        if (onColorChangedListener != null && i2 != this.oldChangedListenerColor) {
            onColorChangedListener.onAlphaChanged(i2);
            this.oldChangedListenerColor = i2;
        }
        invalidate();
    }

    public void setOldCenterColor(int i) {
        this.mCenterOldColor = i;
        this.mCenterOldPaint.setColor(i);
        invalidate();
    }

    public int getOldCenterColor() {
        return this.mCenterOldColor;
    }

    public void setShowOldCenterColor(boolean z) {
        this.mShowCenterOldColor = z;
        invalidate();
    }

    public boolean getShowOldCenterColor() {
        return this.mShowCenterOldColor;
    }

    public void changeOpacityBarColor(int i) {
        setNewCenterColor(i);
    }

    public void changeSaturationBarColor(int i) {
        SaturationBar saturationBar = this.mSaturationBar;
        if (saturationBar != null) {
            saturationBar.setColor(i);
        }
    }

    public void changeValueBarColor(int i) {
        ValueBar valueBar = this.mValueBar;
        if (valueBar != null) {
            valueBar.setColor(i);
        }
    }

    public boolean hasValueBar() {
        return this.mValueBar != null;
    }

    public boolean hasSaturationBar() {
        return this.mSaturationBar != null;
    }

    public boolean hasSVBar() {
        return this.mSVbar != null;
    }

    @Override // android.view.View
    protected Parcelable onSaveInstanceState() {
        Parcelable parcelableOnSaveInstanceState = super.onSaveInstanceState();
        Bundle bundle = new Bundle();
        bundle.putParcelable(STATE_PARENT, parcelableOnSaveInstanceState);
        bundle.putFloat(STATE_ANGLE, this.mAngle);
        bundle.putInt("color", this.mCenterOldColor);
        bundle.putBoolean(STATE_SHOW_OLD_COLOR, this.mShowCenterOldColor);
        return bundle;
    }

    @Override // android.view.View
    protected void onRestoreInstanceState(Parcelable parcelable) {
        Bundle bundle = (Bundle) parcelable;
        super.onRestoreInstanceState(bundle.getParcelable(STATE_PARENT));
        this.mAngle = bundle.getFloat(STATE_ANGLE);
        setOldCenterColor(bundle.getInt("color"));
        this.mShowCenterOldColor = bundle.getBoolean(STATE_SHOW_OLD_COLOR);
        int iCalculateColor = calculateColor(this.mAngle);
        this.mPointerColor.setColor(iCalculateColor);
        setNewCenterColor(iCalculateColor);
    }

    public void setTouchAnywhereOnColorWheelEnabled(boolean z) {
        this.mTouchAnywhereOnColorWheelEnabled = z;
    }

    public boolean getTouchAnywhereOnColorWheel() {
        return this.mTouchAnywhereOnColorWheelEnabled;
    }
}