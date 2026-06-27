package cn.com.heaton.shiningmask.ui.widget.image3d;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Camera;
import android.graphics.Canvas;
import android.graphics.Matrix;
import android.graphics.drawable.Drawable;
import android.net.Uri;
import android.util.AttributeSet;
import android.widget.ImageView;

/* JADX INFO: loaded from: classes.dex */
public class RhyImage3DView extends ImageView {
    private static final float BASE_DEEP = 150.0f;
    private static final float BASE_DEGREE = 35.0f;
    private int imageNo;
    private Bitmap mBitmap;
    private Camera mCamera;
    private float mDeep;
    private float mDx;
    private int mIndex;
    private int mLayoutWidth;
    private Matrix mMaxtrix;
    private float mRotateDegree;
    private int mScrollX;
    private int mWidth;

    public RhyImage3DView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mCamera = new Camera();
        this.mMaxtrix = new Matrix();
    }

    public int getImageNo() {
        return this.imageNo;
    }

    public void setImageNo(int i) {
        this.imageNo = i;
    }

    public void initImageViewBitmap() {
        if (this.mBitmap == null) {
            setDrawingCacheEnabled(true);
            buildDrawingCache();
            this.mBitmap = getDrawingCache();
        }
        this.mLayoutWidth = Image3DSwitchView.mWidth;
        this.mWidth = getWidth() + 30;
    }

    public void setRotateData(int i, int i2) {
        this.mIndex = i;
        this.mScrollX = i2;
    }

    public int getmIndex() {
        return this.mIndex;
    }

    public void setmIndex(int i) {
        this.mIndex = i;
    }

    public void recycleBitmap() {
        Bitmap bitmap = this.mBitmap;
        if (bitmap == null || bitmap.isRecycled()) {
            return;
        }
        this.mBitmap.recycle();
    }

    @Override // android.widget.ImageView
    public void setImageResource(int i) {
        super.setImageResource(i);
        this.mBitmap = null;
        initImageViewBitmap();
    }

    @Override // android.widget.ImageView
    public void setImageBitmap(Bitmap bitmap) {
        super.setImageBitmap(bitmap);
        this.mBitmap = null;
        initImageViewBitmap();
    }

    @Override // android.widget.ImageView
    public void setImageDrawable(Drawable drawable) {
        super.setImageDrawable(drawable);
        this.mBitmap = null;
        initImageViewBitmap();
    }

    @Override // android.widget.ImageView
    public void setImageURI(Uri uri) {
        super.setImageURI(uri);
        this.mBitmap = null;
        initImageViewBitmap();
    }

    @Override // android.widget.ImageView, android.view.View
    protected void onDraw(Canvas canvas) {
        if (this.mBitmap == null) {
            super.onDraw(canvas);
            return;
        }
        if (isImageVisible()) {
            computeRotateData();
            this.mCamera.save();
            this.mCamera.translate(0.0f, 0.0f, this.mDeep);
            this.mCamera.rotateY(this.mRotateDegree);
            this.mCamera.getMatrix(this.mMaxtrix);
            this.mCamera.restore();
            this.mMaxtrix.preTranslate(-this.mDx, (-getHeight()) / 2);
            this.mMaxtrix.postTranslate(this.mDx, getHeight() / 2);
            canvas.drawBitmap(this.mBitmap, this.mMaxtrix, null);
        }
    }

    private void computeRotateData() {
        int i = this.mWidth;
        float f = BASE_DEGREE / i;
        float f2 = BASE_DEEP / ((this.mLayoutWidth - i) / 2);
        int i2 = this.mIndex;
        if (i2 == 0) {
            this.mDx = i;
            int i3 = this.mScrollX;
            this.mRotateDegree = 360.0f - (((i * 2) + i3) * f);
            if (i3 < (-i)) {
                this.mDeep = 0.0f;
                return;
            } else {
                this.mDeep = (i + i3) * f2;
                return;
            }
        }
        if (i2 == 1) {
            int i4 = this.mScrollX;
            if (i4 > 0) {
                this.mDx = i;
                this.mRotateDegree = 325.0f - (i4 * f);
                this.mDeep = i4 * f2;
                return;
            } else {
                if (i4 < (-i)) {
                    this.mDx = -30.0f;
                    this.mRotateDegree = ((-i4) - i) * f;
                } else {
                    this.mDx = i;
                    this.mRotateDegree = 360.0f - ((i + i4) * f);
                }
                this.mDeep = 0.0f;
                return;
            }
        }
        if (i2 == 2) {
            int i5 = this.mScrollX;
            if (i5 > 0) {
                this.mDx = i;
                this.mRotateDegree = 360.0f - (i5 * f);
                this.mDeep = 0.0f;
                if (i5 > i) {
                    this.mDeep = (i5 - i) * f2;
                    return;
                }
                return;
            }
            this.mDx = -30.0f;
            this.mRotateDegree = (-i5) * f;
            this.mDeep = 0.0f;
            if (i5 < (-i)) {
                this.mDeep = (-(i + i5)) * f2;
                return;
            }
            return;
        }
        if (i2 != 3) {
            if (i2 != 4) {
                return;
            }
            this.mDx = -30.0f;
            int i6 = this.mScrollX;
            this.mRotateDegree = ((i * 2) - i6) * f;
            if (i6 > i) {
                this.mDeep = 0.0f;
                return;
            } else {
                this.mDeep = (i - i6) * f2;
                return;
            }
        }
        int i7 = this.mScrollX;
        if (i7 < 0) {
            this.mDx = -30.0f;
            this.mRotateDegree = BASE_DEGREE - (i7 * f);
            this.mDeep = (-i7) * f2;
        } else {
            if (i7 > i) {
                this.mDx = i;
                this.mRotateDegree = 360.0f - ((i7 - i) * f);
            } else {
                this.mDx = -30.0f;
                this.mRotateDegree = BASE_DEGREE - (i7 * f);
            }
            this.mDeep = 0.0f;
        }
    }

    private boolean isImageVisible() {
        int i = this.mIndex;
        if (i == 0) {
            int i2 = this.mScrollX;
            int i3 = this.mLayoutWidth;
            int i4 = this.mWidth;
            return i2 < ((i3 - i4) / 2) - i4;
        }
        if (i == 1) {
            return this.mScrollX <= (this.mLayoutWidth - this.mWidth) / 2;
        }
        if (i == 2) {
            int i5 = this.mScrollX;
            int i6 = this.mLayoutWidth;
            int i7 = this.mWidth;
            return i5 <= (i6 / 2) + (i7 / 2) && i5 >= ((-i6) / 2) - (i7 / 2);
        }
        if (i == 3) {
            return this.mScrollX >= (-(this.mLayoutWidth - this.mWidth)) / 2;
        }
        if (i != 4) {
            return false;
        }
        int i8 = this.mScrollX;
        int i9 = this.mWidth;
        return i8 > i9 - ((this.mLayoutWidth - i9) / 2);
    }
}