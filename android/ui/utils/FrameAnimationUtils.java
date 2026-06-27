package cn.com.heaton.shiningmask.ui.utils;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.drawable.BitmapDrawable;
import android.os.Handler;
import android.widget.ImageView;
import cn.com.heaton.shiningmask.R;
import java.lang.ref.SoftReference;

/* JADX INFO: loaded from: classes.dex */
public class FrameAnimationUtils {
    private Context mContext;

    public interface OnAnimationStoppedListener {
        void AnimationStopped();
    }

    public FrameAnimationUtils(Context context) {
        this.mContext = context;
    }

    public FramesSequenceAnimation createAnim(ImageView imageView, int i) {
        LogUtil.d("=====imageView:" + imageView.hashCode());
        return new FramesSequenceAnimation(imageView, i);
    }

    public class FramesSequenceAnimation {
        private Bitmap mBitmap;
        private BitmapFactory.Options mBitmapOptions;
        private int mDelayMillis;
        private OnAnimationStoppedListener mOnAnimationStoppedListener;
        private SoftReference<ImageView> mSoftReferenceImageView;
        private int[] mFrames = {0};
        private Handler mHandler = new Handler();
        private int mIndex = -1;
        private boolean mShouldRun = false;
        private boolean mIsRunning = false;

        public void setResId(int i) {
            this.mFrames = FrameAnimationUtils.this.getData(i);
            this.mIndex = 0;
        }

        public FramesSequenceAnimation(ImageView imageView, int i) {
            this.mSoftReferenceImageView = new SoftReference<>(imageView);
            this.mDelayMillis = i;
            imageView.setImageResource(R.mipmap.anim24_7);
            Bitmap bitmap = ((BitmapDrawable) imageView.getDrawable()).getBitmap();
            this.mBitmap = Bitmap.createBitmap(bitmap.getWidth(), bitmap.getHeight(), bitmap.getConfig());
            BitmapFactory.Options options = new BitmapFactory.Options();
            this.mBitmapOptions = options;
            options.inBitmap = this.mBitmap;
            this.mBitmapOptions.inMutable = true;
            this.mBitmapOptions.inSampleSize = 1;
        }

        /* JADX INFO: Access modifiers changed from: private */
        public int getNext() {
            int i = this.mIndex + 1;
            this.mIndex = i;
            int[] iArr = this.mFrames;
            if (i >= iArr.length) {
                this.mIndex = 0;
            }
            return iArr[this.mIndex];
        }

        public synchronized void start() {
            this.mShouldRun = true;
            if (this.mIsRunning) {
                return;
            }
            this.mHandler.post(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.utils.FrameAnimationUtils.FramesSequenceAnimation.1
                @Override // java.lang.Runnable
                public void run() {
                    Bitmap bitmapDecodeResource;
                    ImageView imageView = (ImageView) FramesSequenceAnimation.this.mSoftReferenceImageView.get();
                    if (!FramesSequenceAnimation.this.mShouldRun || imageView == null) {
                        FramesSequenceAnimation.this.mIsRunning = false;
                        if (FramesSequenceAnimation.this.mOnAnimationStoppedListener != null) {
                            FramesSequenceAnimation.this.mOnAnimationStoppedListener.AnimationStopped();
                            return;
                        }
                        return;
                    }
                    FramesSequenceAnimation.this.mIsRunning = true;
                    FramesSequenceAnimation.this.mHandler.postDelayed(this, FramesSequenceAnimation.this.mDelayMillis);
                    if (imageView.isShown()) {
                        int next = FramesSequenceAnimation.this.getNext();
                        if (FramesSequenceAnimation.this.mBitmap != null) {
                            try {
                                bitmapDecodeResource = BitmapFactory.decodeResource(imageView.getResources(), next, FramesSequenceAnimation.this.mBitmapOptions);
                            } catch (Exception e) {
                                e.printStackTrace();
                                bitmapDecodeResource = null;
                            }
                            if (bitmapDecodeResource != null) {
                                imageView.setImageBitmap(bitmapDecodeResource);
                                return;
                            }
                            imageView.setImageResource(next);
                            FramesSequenceAnimation.this.mBitmap.recycle();
                            FramesSequenceAnimation.this.mBitmap = null;
                            return;
                        }
                        imageView.setImageResource(next);
                    }
                }
            });
        }

        public synchronized void stop() {
            this.mShouldRun = false;
        }

        public void setOnAnimStopListener(OnAnimationStoppedListener onAnimationStoppedListener) {
            this.mOnAnimationStoppedListener = onAnimationStoppedListener;
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public int[] getData(int i) {
        TypedArray typedArrayObtainTypedArray = this.mContext.getResources().obtainTypedArray(i);
        int length = typedArrayObtainTypedArray.length();
        int[] iArr = new int[typedArrayObtainTypedArray.length()];
        for (int i2 = 0; i2 < length; i2++) {
            iArr[i2] = typedArrayObtainTypedArray.getResourceId(i2, 0);
        }
        typedArrayObtainTypedArray.recycle();
        return iArr;
    }
}