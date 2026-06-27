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
import cn.com.heaton.shiningmask.ui.widget.RhythmLedView;
import cn.com.heaton.shiningmask.ui.widget.loopviewpager.ViewPagerAllResponse;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityRhythmBinding implements ViewBinding {
    public final ImageView imgvLast;
    public final ImageView imgvNext;
    public final ImageView imgvPlayMode;
    public final ImageView imgvPlayPause;
    public final ImageView ivPlayList;
    public final ImageView ivRhyImageBg2;
    public final ImageView ivRhybgTop;
    public final LinearLayout llPlayBtn;
    public final LinearLayout llSongInfo;
    public final LinearLayout lnrlaoActionTwo;
    public final RelativeLayout lnrlaoVolume;
    public final ListView lstvSong;
    public final RhythmLedView rhyledview1;
    public final RhythmLedView rhyledview2;
    public final RelativeLayout rlBottom;
    public final RelativeLayout rlPlay;
    public final RelativeLayout rlRhyBg;
    public final RelativeLayout rlRhySelect;
    public final RelativeLayout rlRhyShow;
    public final RelativeLayout rlRoot;
    public final RelativeLayout rlSongList;
    public final RelativeLayout rlSongName;
    public final RelativeLayout rlSongName1;
    private final RelativeLayout rootView;
    public final SeekBar sbPlayTime;
    public final LayoutTitlebar1Binding top;
    public final TextView txvPlayDuration;
    public final TextView txvPlayTime;
    public final TextView txvSinger;
    public final TextView txvSinger1;
    public final TextView txvSongName;
    public final TextView txvSongName1;
    public final ViewPagerAllResponse vpRhyhm;

    private ActivityRhythmBinding(RelativeLayout relativeLayout, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, ImageView imageView6, ImageView imageView7, LinearLayout linearLayout, LinearLayout linearLayout2, LinearLayout linearLayout3, RelativeLayout relativeLayout2, ListView listView, RhythmLedView rhythmLedView, RhythmLedView rhythmLedView2, RelativeLayout relativeLayout3, RelativeLayout relativeLayout4, RelativeLayout relativeLayout5, RelativeLayout relativeLayout6, RelativeLayout relativeLayout7, RelativeLayout relativeLayout8, RelativeLayout relativeLayout9, RelativeLayout relativeLayout10, RelativeLayout relativeLayout11, SeekBar seekBar, LayoutTitlebar1Binding layoutTitlebar1Binding, TextView textView, TextView textView2, TextView textView3, TextView textView4, TextView textView5, TextView textView6, ViewPagerAllResponse viewPagerAllResponse) {
        this.rootView = relativeLayout;
        this.imgvLast = imageView;
        this.imgvNext = imageView2;
        this.imgvPlayMode = imageView3;
        this.imgvPlayPause = imageView4;
        this.ivPlayList = imageView5;
        this.ivRhyImageBg2 = imageView6;
        this.ivRhybgTop = imageView7;
        this.llPlayBtn = linearLayout;
        this.llSongInfo = linearLayout2;
        this.lnrlaoActionTwo = linearLayout3;
        this.lnrlaoVolume = relativeLayout2;
        this.lstvSong = listView;
        this.rhyledview1 = rhythmLedView;
        this.rhyledview2 = rhythmLedView2;
        this.rlBottom = relativeLayout3;
        this.rlPlay = relativeLayout4;
        this.rlRhyBg = relativeLayout5;
        this.rlRhySelect = relativeLayout6;
        this.rlRhyShow = relativeLayout7;
        this.rlRoot = relativeLayout8;
        this.rlSongList = relativeLayout9;
        this.rlSongName = relativeLayout10;
        this.rlSongName1 = relativeLayout11;
        this.sbPlayTime = seekBar;
        this.top = layoutTitlebar1Binding;
        this.txvPlayDuration = textView;
        this.txvPlayTime = textView2;
        this.txvSinger = textView3;
        this.txvSinger1 = textView4;
        this.txvSongName = textView5;
        this.txvSongName1 = textView6;
        this.vpRhyhm = viewPagerAllResponse;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityRhythmBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityRhythmBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_rhythm, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityRhythmBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.imgv_last;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.imgv_next;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.imgv_play_mode;
                ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView3 != null) {
                    i = R.id.imgv_play_pause;
                    ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView4 != null) {
                        i = R.id.iv_play_list;
                        ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView5 != null) {
                            i = R.id.iv_rhy_image_bg2;
                            ImageView imageView6 = (ImageView) ViewBindings.findChildViewById(view, i);
                            if (imageView6 != null) {
                                i = R.id.iv_rhybg_top;
                                ImageView imageView7 = (ImageView) ViewBindings.findChildViewById(view, i);
                                if (imageView7 != null) {
                                    i = R.id.ll_play_btn;
                                    LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                    if (linearLayout != null) {
                                        i = R.id.ll_song_info;
                                        LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                        if (linearLayout2 != null) {
                                            i = R.id.lnrlao_action_two;
                                            LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                            if (linearLayout3 != null) {
                                                i = R.id.lnrlao_volume;
                                                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                if (relativeLayout != null) {
                                                    i = R.id.lstv_song;
                                                    ListView listView = (ListView) ViewBindings.findChildViewById(view, i);
                                                    if (listView != null) {
                                                        i = R.id.rhyledview_1;
                                                        RhythmLedView rhythmLedView = (RhythmLedView) ViewBindings.findChildViewById(view, i);
                                                        if (rhythmLedView != null) {
                                                            i = R.id.rhyledview_2;
                                                            RhythmLedView rhythmLedView2 = (RhythmLedView) ViewBindings.findChildViewById(view, i);
                                                            if (rhythmLedView2 != null) {
                                                                i = R.id.rl_bottom;
                                                                RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                if (relativeLayout2 != null) {
                                                                    i = R.id.rl_play;
                                                                    RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                    if (relativeLayout3 != null) {
                                                                        i = R.id.rl_rhy_bg;
                                                                        RelativeLayout relativeLayout4 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                        if (relativeLayout4 != null) {
                                                                            i = R.id.rl_rhy_select;
                                                                            RelativeLayout relativeLayout5 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                            if (relativeLayout5 != null) {
                                                                                i = R.id.rl_rhy_show;
                                                                                RelativeLayout relativeLayout6 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                if (relativeLayout6 != null) {
                                                                                    i = R.id.rl_root;
                                                                                    RelativeLayout relativeLayout7 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                    if (relativeLayout7 != null) {
                                                                                        i = R.id.rl_song_list;
                                                                                        RelativeLayout relativeLayout8 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                        if (relativeLayout8 != null) {
                                                                                            i = R.id.rl_song_name;
                                                                                            RelativeLayout relativeLayout9 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                            if (relativeLayout9 != null) {
                                                                                                i = R.id.rl_song_name1;
                                                                                                RelativeLayout relativeLayout10 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                if (relativeLayout10 != null) {
                                                                                                    i = R.id.sb_play_time;
                                                                                                    SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, i);
                                                                                                    if (seekBar != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                                                                                                        LayoutTitlebar1Binding layoutTitlebar1BindingBind = LayoutTitlebar1Binding.bind(viewFindChildViewById);
                                                                                                        i = R.id.txv_play_duration;
                                                                                                        TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                        if (textView != null) {
                                                                                                            i = R.id.txv_play_time;
                                                                                                            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                            if (textView2 != null) {
                                                                                                                i = R.id.txv_singer;
                                                                                                                TextView textView3 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                if (textView3 != null) {
                                                                                                                    i = R.id.txv_singer1;
                                                                                                                    TextView textView4 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                    if (textView4 != null) {
                                                                                                                        i = R.id.txv_song_name;
                                                                                                                        TextView textView5 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                        if (textView5 != null) {
                                                                                                                            i = R.id.txv_song_name1;
                                                                                                                            TextView textView6 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                            if (textView6 != null) {
                                                                                                                                i = R.id.vp_rhyhm;
                                                                                                                                ViewPagerAllResponse viewPagerAllResponse = (ViewPagerAllResponse) ViewBindings.findChildViewById(view, i);
                                                                                                                                if (viewPagerAllResponse != null) {
                                                                                                                                    return new ActivityRhythmBinding((RelativeLayout) view, imageView, imageView2, imageView3, imageView4, imageView5, imageView6, imageView7, linearLayout, linearLayout2, linearLayout3, relativeLayout, listView, rhythmLedView, rhythmLedView2, relativeLayout2, relativeLayout3, relativeLayout4, relativeLayout5, relativeLayout6, relativeLayout7, relativeLayout8, relativeLayout9, relativeLayout10, seekBar, layoutTitlebar1BindingBind, textView, textView2, textView3, textView4, textView5, textView6, viewPagerAllResponse);
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