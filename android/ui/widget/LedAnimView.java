package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Color;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;
import com.alibaba.fastjson2.internal.asm.Opcodes;

/* JADX INFO: loaded from: classes.dex */
public class LedAnimView extends ViewGroup {
    public static final int MODE_ERASER = 2;
    public static final int MODE_NO = 0;
    public static final int MODE_PAINT = 1;
    public static final int ORIENTATION_LANDSCAPE = 1;
    public static final int ORIENTATION_PORTRAIT = 0;
    private static final String TAG = "LedView";
    int[][] animData;
    private boolean flag;
    private int heightCount;
    int heightSize;
    private int index;
    private boolean isDispatchTouch;
    private int measureHeightMode;
    private int measureWidthMode;
    private int mode;
    int moveMax;
    int moveYMax;
    private int offset;
    private int orientation;
    private int pointAllLength;
    private int pointLength;
    private int pointMargin;
    private int pointYAllLength;
    private int selectedColor;
    private int selectedColor2;
    private int selectedColor3;
    private int selectedColorGray;
    private int selectedColorGray2;
    private int selectedColorGray3;
    private byte[] textData;
    private int unSelectedColor;
    private int widthCount;
    int widthSize;
    private int xMore;
    private int yMore;

    public LedAnimView(Context context) {
        super(context);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor3 = Color.argb(89, 82, 227, 255);
        this.selectedColorGray = Color.rgb(42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.argb(89, 42, 122, 142);
        this.unSelectedColor = 0;
        this.isDispatchTouch = true;
        this.index = 0;
    }

    public LedAnimView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor3 = Color.argb(89, 82, 227, 255);
        this.selectedColorGray = Color.rgb(42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.argb(89, 42, 122, 142);
        this.unSelectedColor = 0;
        this.isDispatchTouch = true;
        this.index = 0;
    }

    public LedAnimView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor = Color.rgb(82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor3 = Color.argb(89, 82, 227, 255);
        this.selectedColorGray = Color.rgb(42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.argb(89, 42, 122, 142);
        this.unSelectedColor = 0;
        this.isDispatchTouch = true;
        this.index = 0;
    }

    public void init(int i, int i2) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            LedItemView1 ledItemView1 = new LedItemView1(getContext());
            ledItemView1.setViewNumber(i3);
            ledItemView1.setColumnNumber(i3 / i2);
            ledItemView1.setRowNumber(i3 % i2);
            ledItemView1.setPaint(this.unSelectedColor);
            ledItemView1.postInvalidate();
            addView(ledItemView1);
        }
    }

    public void init(int i, int i2, float f) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            LedItemView1 ledItemView1 = new LedItemView1(getContext());
            ledItemView1.setViewNumber(i3);
            ledItemView1.setColumnNumber(i3 / i2);
            ledItemView1.setRowNumber(i3 % i2);
            ledItemView1.setPaint(this.unSelectedColor, f);
            ledItemView1.postInvalidate();
            addView(ledItemView1);
        }
    }

    public void removeAllChildView() {
        int childCount = getChildCount();
        if (childCount > 0) {
            for (int i = 0; i < childCount; i++) {
                removeViewAt(0);
            }
        }
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        measureChildren(i, i2);
        this.widthSize = View.MeasureSpec.getSize(i);
        this.heightSize = View.MeasureSpec.getSize(i2);
        this.measureWidthMode = View.MeasureSpec.getMode(i);
        this.measureHeightMode = View.MeasureSpec.getMode(i2);
        int i3 = this.widthSize;
        int i4 = this.widthCount;
        int i5 = i3 % i4;
        this.xMore = i5;
        int i6 = this.heightSize;
        int i7 = this.heightCount;
        this.yMore = i6 % i7;
        int i8 = i3 / i4;
        this.pointAllLength = i8;
        int i9 = i6 / i7;
        this.pointYAllLength = i9;
        int i10 = this.pointMargin;
        this.pointLength = i8 - (i10 * 2);
        int i11 = i5 / 2;
        this.offset = i11;
        this.moveMax = (i8 * i7) - i6;
        this.moveYMax = (i9 * i4) - i3;
        int i12 = (i8 * i7) - (i10 + i11);
        this.heightSize = i12;
        setMeasuredDimension(i3, i12);
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        int childCount = getChildCount();
        for (int i5 = 0; i5 < childCount; i5++) {
            View childAt = getChildAt(i5);
            int i6 = this.heightCount;
            int i7 = this.pointAllLength;
            int i8 = this.pointMargin;
            int i9 = ((i5 / i6) * i7) + i8 + this.offset;
            int i10 = ((i5 % i6) * i7) + i8;
            int i11 = this.pointLength;
            childAt.layout(i9, i10, i9 + i11, i11 + i10);
        }
    }

    public void clearSelected() {
        for (int i = 0; i < getChildCount(); i++) {
            LedItemView1 ledItemView1 = (LedItemView1) getChildAt(i);
            ledItemView1.setChecked(false);
            ledItemView1.setPaint(this.unSelectedColor);
            ledItemView1.postInvalidate();
        }
    }

    public void setItemSelected(int i, boolean z) {
        if (i < 0 || i >= getChildCount()) {
            return;
        }
        LedItemView1 ledItemView1 = (LedItemView1) getChildAt(i);
        if (z) {
            ledItemView1.setChecked(true);
            ledItemView1.setPaint(this.selectedColor);
            ledItemView1.postInvalidate();
        } else {
            ledItemView1.setChecked(false);
            ledItemView1.setPaint(this.unSelectedColor);
            ledItemView1.postInvalidate();
        }
    }

    public void setItemSelectedColor(int i) {
        if (i == 1) {
            return;
        }
        this.selectedColor = Color.rgb(82, 227, 255);
    }

    public void setData(byte[] bArr) {
        if (bArr == null) {
            return;
        }
        int childCount = getChildCount();
        for (int i = 0; i < childCount; i++) {
            LedItemView1 ledItemView1 = (LedItemView1) getChildAt(i);
            if (bArr[i] == 1) {
                ledItemView1.setChecked(true);
                ledItemView1.setPaint(this.selectedColor);
            } else {
                ledItemView1.setChecked(false);
                ledItemView1.setPaint(this.unSelectedColor);
            }
            ledItemView1.postInvalidate();
        }
    }

    private LedItemView1 getLedItemView1(int i, int i2) {
        return (LedItemView1) getChildAt((i * 9) + (9 - (i2 + 1)));
    }

    public void setImageData(int[][] iArr) {
        this.animData = iArr;
        showImage(iArr);
    }

    private void showImage(int[][] iArr) {
        if (this.index >= iArr.length) {
            this.index = 0;
        }
        int[] iArr2 = iArr[this.index];
        for (int i = 0; i < iArr2.length; i++) {
            for (int i2 = 0; i2 < 9; i2++) {
                int i3 = (iArr2[i] >> (i2 * 2)) & 3;
                LedItemView1 ledItemView1 = getLedItemView1(i, i2);
                if (ledItemView1 != null) {
                    if (i3 == 1) {
                        ledItemView1.setChecked(true);
                        ledItemView1.setPaint(this.selectedColorGray3);
                    } else if (i3 == 2) {
                        ledItemView1.setChecked(true);
                        ledItemView1.setPaint(this.selectedColorGray2);
                    } else if (i3 == 3) {
                        ledItemView1.setChecked(true);
                        ledItemView1.setPaint(this.selectedColorGray);
                    } else {
                        ledItemView1.setChecked(false);
                        ledItemView1.setPaint(this.unSelectedColor);
                    }
                    ledItemView1.postInvalidate();
                }
            }
        }
        this.index++;
    }

    public void setSelect(boolean z) {
        if (z) {
            this.selectedColorGray3 = this.selectedColor3;
            this.selectedColorGray2 = this.selectedColor2;
            this.selectedColorGray = this.selectedColor;
        } else {
            this.selectedColorGray = Color.rgb(42, 122, 142);
            this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
            this.selectedColorGray3 = Color.argb(89, 42, 122, 142);
        }
    }

    public int getWidthCount() {
        return this.widthCount;
    }

    public int getHeightCount() {
        return this.heightCount;
    }

    public void setPointMargin(int i) {
        this.pointMargin = i;
    }

    public int getMoveMax() {
        return this.moveMax;
    }

    public void setUnSelectedColor(int i) {
        this.unSelectedColor = i;
    }

    public void setSelectedColor(int i) {
        this.selectedColor = i;
    }

    public void setMode(int i) {
        this.mode = i;
    }

    public int getOrientation() {
        return this.orientation;
    }

    public void setOrientation(int i) {
        this.orientation = i;
    }
}