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
import cn.com.heaton.shiningmask.ui.activity.test.RhythmLedView;
import cn.com.heaton.shiningmask.ui.widget.image3d.RhyImage3DSwitchView;
import cn.com.heaton.shiningmask.ui.widget.image3d.RhyImage3DView;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityRhythm3Binding implements ViewBinding {
    public final RhyImage3DView image1;
    public final RhyImage3DView image11;
    public final RhyImage3DView image12;
    public final RhyImage3DView image14;
    public final RhyImage3DView image15;
    public final RhyImage3DView image2;
    public final RhyImage3DView image4;
    public final RhyImage3DView image5;
    public final RhyImage3DView image6;
    public final RhyImage3DView image7;
    public final RhyImage3DSwitchView imageSwitchView;
    public final ImageView imgvLast;
    public final ImageView imgvNext;
    public final ImageView imgvPlayMode;
    public final ImageView imgvPlayPause;
    public final ImageView ivAnim;
    public final ImageView ivBtn1;
    public final ImageView ivPlayList;
    public final LinearLayout llPlayBtn;
    public final LinearLayout llSongInfo;
    public final LinearLayout lnrlaoActionTwo;
    public final RelativeLayout lnrlaoVolume;
    public final ListView lstvSong;
    public final RhythmLedView rhyledview1;
    public final RelativeLayout rlBottom;
    public final RelativeLayout rlPlay;
    public final RelativeLayout rlRhySelect;
    public final RelativeLayout rlRhyShow;
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

    private ActivityRhythm3Binding(RelativeLayout relativeLayout, RhyImage3DView rhyImage3DView, RhyImage3DView rhyImage3DView2, RhyImage3DView rhyImage3DView3, RhyImage3DView rhyImage3DView4, RhyImage3DView rhyImage3DView5, RhyImage3DView rhyImage3DView6, RhyImage3DView rhyImage3DView7, RhyImage3DView rhyImage3DView8, RhyImage3DView rhyImage3DView9, RhyImage3DView rhyImage3DView10, RhyImage3DSwitchView rhyImage3DSwitchView, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, ImageView imageView6, ImageView imageView7, LinearLayout linearLayout, LinearLayout linearLayout2, LinearLayout linearLayout3, RelativeLayout relativeLayout2, ListView listView, RhythmLedView rhythmLedView, RelativeLayout relativeLayout3, RelativeLayout relativeLayout4, RelativeLayout relativeLayout5, RelativeLayout relativeLayout6, RelativeLayout relativeLayout7, RelativeLayout relativeLayout8, RelativeLayout relativeLayout9, SeekBar seekBar, LayoutTitlebar1Binding layoutTitlebar1Binding, TextView textView, TextView textView2, TextView textView3, TextView textView4, TextView textView5, TextView textView6) {
        this.rootView = relativeLayout;
        this.image1 = rhyImage3DView;
        this.image11 = rhyImage3DView2;
        this.image12 = rhyImage3DView3;
        this.image14 = rhyImage3DView4;
        this.image15 = rhyImage3DView5;
        this.image2 = rhyImage3DView6;
        this.image4 = rhyImage3DView7;
        this.image5 = rhyImage3DView8;
        this.image6 = rhyImage3DView9;
        this.image7 = rhyImage3DView10;
        this.imageSwitchView = rhyImage3DSwitchView;
        this.imgvLast = imageView;
        this.imgvNext = imageView2;
        this.imgvPlayMode = imageView3;
        this.imgvPlayPause = imageView4;
        this.ivAnim = imageView5;
        this.ivBtn1 = imageView6;
        this.ivPlayList = imageView7;
        this.llPlayBtn = linearLayout;
        this.llSongInfo = linearLayout2;
        this.lnrlaoActionTwo = linearLayout3;
        this.lnrlaoVolume = relativeLayout2;
        this.lstvSong = listView;
        this.rhyledview1 = rhythmLedView;
        this.rlBottom = relativeLayout3;
        this.rlPlay = relativeLayout4;
        this.rlRhySelect = relativeLayout5;
        this.rlRhyShow = relativeLayout6;
        this.rlSongList = relativeLayout7;
        this.rlSongName = relativeLayout8;
        this.rlSongName1 = relativeLayout9;
        this.sbPlayTime = seekBar;
        this.top = layoutTitlebar1Binding;
        this.txvPlayDuration = textView;
        this.txvPlayTime = textView2;
        this.txvSinger = textView3;
        this.txvSinger1 = textView4;
        this.txvSongName = textView5;
        this.txvSongName1 = textView6;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityRhythm3Binding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityRhythm3Binding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_rhythm3, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityRhythm3Binding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.image1;
        RhyImage3DView rhyImage3DView = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
        if (rhyImage3DView != null) {
            i = R.id.image_1_1;
            RhyImage3DView rhyImage3DView2 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
            if (rhyImage3DView2 != null) {
                i = R.id.image_1_2;
                RhyImage3DView rhyImage3DView3 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
                if (rhyImage3DView3 != null) {
                    i = R.id.image_1_4;
                    RhyImage3DView rhyImage3DView4 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
                    if (rhyImage3DView4 != null) {
                        i = R.id.image_1_5;
                        RhyImage3DView rhyImage3DView5 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
                        if (rhyImage3DView5 != null) {
                            i = R.id.image2;
                            RhyImage3DView rhyImage3DView6 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
                            if (rhyImage3DView6 != null) {
                                i = R.id.image4;
                                RhyImage3DView rhyImage3DView7 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
                                if (rhyImage3DView7 != null) {
                                    i = R.id.image5;
                                    RhyImage3DView rhyImage3DView8 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
                                    if (rhyImage3DView8 != null) {
                                        i = R.id.image6;
                                        RhyImage3DView rhyImage3DView9 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
                                        if (rhyImage3DView9 != null) {
                                            i = R.id.image7;
                                            RhyImage3DView rhyImage3DView10 = (RhyImage3DView) ViewBindings.findChildViewById(view, i);
                                            if (rhyImage3DView10 != null) {
                                                i = R.id.image_switch_view;
                                                RhyImage3DSwitchView rhyImage3DSwitchView = (RhyImage3DSwitchView) ViewBindings.findChildViewById(view, i);
                                                if (rhyImage3DSwitchView != null) {
                                                    i = R.id.imgv_last;
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
                                                                    i = R.id.iv_anim;
                                                                    ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                    if (imageView5 != null) {
                                                                        i = R.id.iv_btn1;
                                                                        ImageView imageView6 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                        if (imageView6 != null) {
                                                                            i = R.id.iv_play_list;
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
                                                                                                        i = R.id.rl_bottom;
                                                                                                        RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                        if (relativeLayout2 != null) {
                                                                                                            i = R.id.rl_play;
                                                                                                            RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                            if (relativeLayout3 != null) {
                                                                                                                i = R.id.rl_rhy_select;
                                                                                                                RelativeLayout relativeLayout4 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                if (relativeLayout4 != null) {
                                                                                                                    i = R.id.rl_rhy_show;
                                                                                                                    RelativeLayout relativeLayout5 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                    if (relativeLayout5 != null) {
                                                                                                                        i = R.id.rl_song_list;
                                                                                                                        RelativeLayout relativeLayout6 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                        if (relativeLayout6 != null) {
                                                                                                                            i = R.id.rl_song_name;
                                                                                                                            RelativeLayout relativeLayout7 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                            if (relativeLayout7 != null) {
                                                                                                                                i = R.id.rl_song_name1;
                                                                                                                                RelativeLayout relativeLayout8 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                if (relativeLayout8 != null) {
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
                                                                                                                                                                return new ActivityRhythm3Binding((RelativeLayout) view, rhyImage3DView, rhyImage3DView2, rhyImage3DView3, rhyImage3DView4, rhyImage3DView5, rhyImage3DView6, rhyImage3DView7, rhyImage3DView8, rhyImage3DView9, rhyImage3DView10, rhyImage3DSwitchView, imageView, imageView2, imageView3, imageView4, imageView5, imageView6, imageView7, linearLayout, linearLayout2, linearLayout3, relativeLayout, listView, rhythmLedView, relativeLayout2, relativeLayout3, relativeLayout4, relativeLayout5, relativeLayout6, relativeLayout7, relativeLayout8, seekBar, layoutTitlebar1BindingBind, textView, textView2, textView3, textView4, textView5, textView6);
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