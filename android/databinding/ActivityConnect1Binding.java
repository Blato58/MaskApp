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
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityConnect1Binding implements ViewBinding {
    public final ImageView ivDeviceConnect;
    public final ImageView ivLightIcon1;
    public final ImageView ivLogo;
    public final ImageView ivRefresh;
    public final ImageView ivSetting;
    public final RecyclerView listHorizontalMenu;
    public final LinearLayout llBottom;
    public final LinearLayout llConnectDevice;
    public final ListView lvDevice;
    public final RelativeLayout rlCenterMenu;
    public final RelativeLayout rlColour;
    public final RelativeLayout rlDeviceConnect;
    public final RelativeLayout rlRoot;
    public final RelativeLayout rlTitleBar;
    private final RelativeLayout rootView;
    public final SeekBar sbColour;
    public final TextView tvProgressLight;

    private ActivityConnect1Binding(RelativeLayout relativeLayout, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, RecyclerView recyclerView, LinearLayout linearLayout, LinearLayout linearLayout2, ListView listView, RelativeLayout relativeLayout2, RelativeLayout relativeLayout3, RelativeLayout relativeLayout4, RelativeLayout relativeLayout5, RelativeLayout relativeLayout6, SeekBar seekBar, TextView textView) {
        this.rootView = relativeLayout;
        this.ivDeviceConnect = imageView;
        this.ivLightIcon1 = imageView2;
        this.ivLogo = imageView3;
        this.ivRefresh = imageView4;
        this.ivSetting = imageView5;
        this.listHorizontalMenu = recyclerView;
        this.llBottom = linearLayout;
        this.llConnectDevice = linearLayout2;
        this.lvDevice = listView;
        this.rlCenterMenu = relativeLayout2;
        this.rlColour = relativeLayout3;
        this.rlDeviceConnect = relativeLayout4;
        this.rlRoot = relativeLayout5;
        this.rlTitleBar = relativeLayout6;
        this.sbColour = seekBar;
        this.tvProgressLight = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityConnect1Binding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityConnect1Binding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_connect1, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityConnect1Binding bind(View view) {
        int i = R.id.iv_device_connect;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.iv_light_icon_1;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.iv_logo;
                ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView3 != null) {
                    i = R.id.iv_refresh;
                    ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView4 != null) {
                        i = R.id.iv_setting;
                        ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView5 != null) {
                            i = R.id.list_horizontal_menu;
                            RecyclerView recyclerView = (RecyclerView) ViewBindings.findChildViewById(view, i);
                            if (recyclerView != null) {
                                i = R.id.ll_bottom;
                                LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                if (linearLayout != null) {
                                    i = R.id.ll_connect_device;
                                    LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                    if (linearLayout2 != null) {
                                        i = R.id.lv_device;
                                        ListView listView = (ListView) ViewBindings.findChildViewById(view, i);
                                        if (listView != null) {
                                            i = R.id.rl_center_menu;
                                            RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                            if (relativeLayout != null) {
                                                i = R.id.rl_colour;
                                                RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                if (relativeLayout2 != null) {
                                                    i = R.id.rl_device_connect;
                                                    RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                    if (relativeLayout3 != null) {
                                                        RelativeLayout relativeLayout4 = (RelativeLayout) view;
                                                        i = R.id.rl_title_bar;
                                                        RelativeLayout relativeLayout5 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                        if (relativeLayout5 != null) {
                                                            i = R.id.sb_colour;
                                                            SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, i);
                                                            if (seekBar != null) {
                                                                i = R.id.tv_progress_light;
                                                                TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                                                                if (textView != null) {
                                                                    return new ActivityConnect1Binding(relativeLayout4, imageView, imageView2, imageView3, imageView4, imageView5, recyclerView, linearLayout, linearLayout2, listView, relativeLayout, relativeLayout2, relativeLayout3, relativeLayout4, relativeLayout5, seekBar, textView);
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