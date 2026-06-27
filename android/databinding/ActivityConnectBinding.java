package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityConnectBinding implements ViewBinding {
    public final ImageView ivAddDevice;
    public final RelativeLayout ivBottom;
    public final ImageView ivCenter;
    public final ImageView ivCenter1;
    public final ImageView ivDeviceConnect;
    public final ImageView ivLightIcon1;
    public final ImageView ivRefresh;
    public final ImageView ivSetting;
    public final LinearLayout llBottom;
    public final RelativeLayout llCenter;
    public final LinearLayout llTop1;
    public final LinearLayout llVersion;
    public final ListView lvDevice;
    public final ListView lvLanguage;
    public final RelativeLayout rlCenter;
    public final RelativeLayout rlCenterMenu;
    public final RelativeLayout rlColour;
    public final RelativeLayout rlRoot;
    private final RelativeLayout rootView;
    public final SeekBar sbColour;
    public final TextView tvProgressLight;
    public final TextView tvSettingsTitle;
    public final TextView tvVersion;

    private ActivityConnectBinding(RelativeLayout relativeLayout, ImageView imageView, RelativeLayout relativeLayout2, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, ImageView imageView6, ImageView imageView7, LinearLayout linearLayout, RelativeLayout relativeLayout3, LinearLayout linearLayout2, LinearLayout linearLayout3, ListView listView, ListView listView2, RelativeLayout relativeLayout4, RelativeLayout relativeLayout5, RelativeLayout relativeLayout6, RelativeLayout relativeLayout7, SeekBar seekBar, TextView textView, TextView textView2, TextView textView3) {
        this.rootView = relativeLayout;
        this.ivAddDevice = imageView;
        this.ivBottom = relativeLayout2;
        this.ivCenter = imageView2;
        this.ivCenter1 = imageView3;
        this.ivDeviceConnect = imageView4;
        this.ivLightIcon1 = imageView5;
        this.ivRefresh = imageView6;
        this.ivSetting = imageView7;
        this.llBottom = linearLayout;
        this.llCenter = relativeLayout3;
        this.llTop1 = linearLayout2;
        this.llVersion = linearLayout3;
        this.lvDevice = listView;
        this.lvLanguage = listView2;
        this.rlCenter = relativeLayout4;
        this.rlCenterMenu = relativeLayout5;
        this.rlColour = relativeLayout6;
        this.rlRoot = relativeLayout7;
        this.sbColour = seekBar;
        this.tvProgressLight = textView;
        this.tvSettingsTitle = textView2;
        this.tvVersion = textView3;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityConnectBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityConnectBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_connect, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityConnectBinding bind(View view) {
        int i = R.id.iv_add_device;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.iv_bottom;
            RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
            if (relativeLayout != null) {
                i = R.id.iv_center;
                ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView2 != null) {
                    i = R.id.iv_center1;
                    ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView3 != null) {
                        i = R.id.iv_device_connect;
                        ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView4 != null) {
                            i = R.id.iv_light_icon_1;
                            ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                            if (imageView5 != null) {
                                i = R.id.iv_refresh;
                                ImageView imageView6 = (ImageView) ViewBindings.findChildViewById(view, i);
                                if (imageView6 != null) {
                                    i = R.id.iv_setting;
                                    ImageView imageView7 = (ImageView) ViewBindings.findChildViewById(view, i);
                                    if (imageView7 != null) {
                                        i = R.id.ll_bottom;
                                        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                        if (linearLayout != null) {
                                            i = R.id.ll_center;
                                            RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                            if (relativeLayout2 != null) {
                                                i = R.id.ll_top1;
                                                LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                if (linearLayout2 != null) {
                                                    i = R.id.ll_version;
                                                    LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                    if (linearLayout3 != null) {
                                                        i = R.id.lv_device;
                                                        ListView listView = (ListView) ViewBindings.findChildViewById(view, i);
                                                        if (listView != null) {
                                                            i = R.id.lv_language;
                                                            ListView listView2 = (ListView) ViewBindings.findChildViewById(view, i);
                                                            if (listView2 != null) {
                                                                i = R.id.rl_center;
                                                                RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                if (relativeLayout3 != null) {
                                                                    i = R.id.rl_center_menu;
                                                                    RelativeLayout relativeLayout4 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                    if (relativeLayout4 != null) {
                                                                        i = R.id.rl_colour;
                                                                        RelativeLayout relativeLayout5 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                        if (relativeLayout5 != null) {
                                                                            RelativeLayout relativeLayout6 = (RelativeLayout) view;
                                                                            i = R.id.sb_colour;
                                                                            SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, i);
                                                                            if (seekBar != null) {
                                                                                i = R.id.tv_progress_light;
                                                                                TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                if (textView != null) {
                                                                                    i = R.id.tv_settings_title;
                                                                                    TextView textView2 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                    if (textView2 != null) {
                                                                                        i = R.id.tv_version;
                                                                                        TextView textView3 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                        if (textView3 != null) {
                                                                                            return new ActivityConnectBinding(relativeLayout6, imageView, relativeLayout, imageView2, imageView3, imageView4, imageView5, imageView6, imageView7, linearLayout, relativeLayout2, linearLayout2, linearLayout3, listView, listView2, relativeLayout3, relativeLayout4, relativeLayout5, relativeLayout6, seekBar, textView, textView2, textView3);
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}