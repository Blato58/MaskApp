package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityCamreLayoutBinding implements ViewBinding {
    public final FrameLayout cameraPreviewLayout;
    public final ImageView cancleButton;
    public final ImageView ivRight;
    public final RelativeLayout llPhotoLayout;
    public final ImageView maskImg;
    private final RelativeLayout rootView;
    public final ImageView takePhotoButton;

    private ActivityCamreLayoutBinding(RelativeLayout relativeLayout, FrameLayout frameLayout, ImageView imageView, ImageView imageView2, RelativeLayout relativeLayout2, ImageView imageView3, ImageView imageView4) {
        this.rootView = relativeLayout;
        this.cameraPreviewLayout = frameLayout;
        this.cancleButton = imageView;
        this.ivRight = imageView2;
        this.llPhotoLayout = relativeLayout2;
        this.maskImg = imageView3;
        this.takePhotoButton = imageView4;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityCamreLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityCamreLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_camre_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityCamreLayoutBinding bind(View view) {
        int i = R.id.camera_preview_layout;
        FrameLayout frameLayout = (FrameLayout) ViewBindings.findChildViewById(view, i);
        if (frameLayout != null) {
            i = R.id.cancle_button;
            ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView != null) {
                i = R.id.iv_right;
                ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView2 != null) {
                    i = R.id.ll_photo_layout;
                    RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                    if (relativeLayout != null) {
                        i = R.id.mask_img;
                        ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView3 != null) {
                            i = R.id.take_photo_button;
                            ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                            if (imageView4 != null) {
                                return new ActivityCamreLayoutBinding((RelativeLayout) view, frameLayout, imageView, imageView2, relativeLayout, imageView3, imageView4);
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}