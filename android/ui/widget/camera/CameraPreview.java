package cn.com.heaton.shiningmask.ui.widget.camera;

import android.app.Activity;
import android.content.Context;
import android.hardware.Camera;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import com.alibaba.fastjson2.internal.asm.Opcodes;
import java.io.IOException;
import java.util.Iterator;
import java.util.SortedSet;

/* JADX INFO: loaded from: classes.dex */
public class CameraPreview extends SurfaceView implements SurfaceHolder.Callback {
    private Context context;
    private boolean isPreview;
    private AspectRatio mAspectRatio;
    private Camera mCamera;
    private int mDisplayOrientation;
    private SurfaceHolder mHolder;
    private final SizeMap mPictureSizes;
    private final SizeMap mPreviewSizes;

    private boolean isLandscape(int i) {
        return i == 1 || i == 3;
    }

    public CameraPreview(Context context, Camera camera) {
        super(context);
        this.mPreviewSizes = new SizeMap();
        this.mPictureSizes = new SizeMap();
        this.context = context;
        this.mCamera = camera;
        SurfaceHolder holder = getHolder();
        this.mHolder = holder;
        holder.addCallback(this);
        this.mHolder.setType(3);
        this.mDisplayOrientation = ((Activity) context).getWindowManager().getDefaultDisplay().getRotation();
        this.mAspectRatio = AspectRatio.of(9, 16);
    }

    @Override // android.view.SurfaceHolder.Callback
    public void surfaceCreated(SurfaceHolder surfaceHolder) {
        try {
            this.mAspectRatio = getDeviceAspectRatio((Activity) this.context);
            this.mCamera.setDisplayOrientation(getDisplayOrientation());
            Camera.Parameters parameters = this.mCamera.getParameters();
            this.mPreviewSizes.clear();
            for (Camera.Size size : parameters.getSupportedPreviewSizes()) {
                this.mPreviewSizes.add(new Size(Math.min(size.width, size.height), Math.max(size.width, size.height)));
            }
            this.mPictureSizes.clear();
            for (Camera.Size size2 : parameters.getSupportedPictureSizes()) {
                this.mPictureSizes.add(new Size(Math.min(size2.width, size2.height), Math.max(size2.width, size2.height)));
            }
            Size sizeChooseOptimalSize = chooseOptimalSize(this.mPreviewSizes.sizes(this.mAspectRatio));
            Size sizeLast = this.mPictureSizes.sizes(this.mAspectRatio).last();
            parameters.setPreviewSize(Math.max(sizeChooseOptimalSize.getWidth(), sizeChooseOptimalSize.getHeight()), Math.min(sizeChooseOptimalSize.getWidth(), sizeChooseOptimalSize.getHeight()));
            parameters.setPictureSize(Math.max(sizeLast.getWidth(), sizeLast.getHeight()), Math.min(sizeLast.getWidth(), sizeLast.getHeight()));
            parameters.setPictureFormat(256);
            parameters.setRotation(getDisplayOrientation());
            this.mCamera.setParameters(parameters);
            this.mCamera.setPreviewDisplay(surfaceHolder);
            this.mCamera.startPreview();
            this.isPreview = true;
        } catch (Exception e) {
            Log.e("CameraPreview", "相机预览错误: " + e.getMessage());
        }
    }

    @Override // android.view.SurfaceHolder.Callback
    public void surfaceChanged(SurfaceHolder surfaceHolder, int i, int i2, int i3) {
        if (surfaceHolder.getSurface() == null) {
            return;
        }
        Camera camera = this.mCamera;
        if (camera != null && this.isPreview) {
            try {
                camera.stopPreview();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        try {
            this.mCamera.setPreviewDisplay(this.mHolder);
        } catch (IOException e2) {
            e2.printStackTrace();
        }
        this.mCamera.startPreview();
    }

    @Override // android.view.SurfaceHolder.Callback
    public void surfaceDestroyed(SurfaceHolder surfaceHolder) {
        Camera camera = this.mCamera;
        if (camera == null || !this.isPreview || camera == null) {
            return;
        }
        try {
            camera.setPreviewCallback(null);
            this.mCamera.stopPreview();
            this.mCamera.release();
        } catch (Exception unused) {
        }
    }

    private AspectRatio getDeviceAspectRatio(Activity activity) {
        int width = activity.getWindow().getDecorView().getWidth();
        int height = activity.getWindow().getDecorView().getHeight();
        return AspectRatio.of(Math.min(width, height), Math.max(width, height));
    }

    private Size chooseOptimalSize(SortedSet<Size> sortedSet) {
        int width = getWidth();
        int height = getHeight();
        if (isLandscape(this.mDisplayOrientation)) {
            height = width;
            width = height;
        }
        Size size = new Size(width, height);
        if (sortedSet != null && !sortedSet.isEmpty()) {
            Iterator<Size> it = sortedSet.iterator();
            while (it.hasNext()) {
                size = it.next();
                if (width <= size.getWidth() && height <= size.getHeight()) {
                    break;
                }
            }
        }
        return size;
    }

    private int getDisplayOrientation() {
        Camera.CameraInfo cameraInfo = new Camera.CameraInfo();
        int i = 0;
        Camera.getCameraInfo(0, cameraInfo);
        int i2 = this.mDisplayOrientation;
        if (i2 != 0) {
            if (i2 == 1) {
                i = 90;
            } else if (i2 == 2) {
                i = Opcodes.GETFIELD;
            } else if (i2 == 3) {
                i = 270;
            }
        }
        int i3 = ((i + 45) / 90) * 90;
        if (cameraInfo.facing == 1) {
            return ((cameraInfo.orientation - i3) + 360) % 360;
        }
        return (cameraInfo.orientation + i3) % 360;
    }
}