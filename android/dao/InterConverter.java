package cn.com.heaton.shiningmask.dao;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import java.util.ArrayList;
import org.greenrobot.greendao.converter.PropertyConverter;

/* JADX INFO: loaded from: classes.dex */
public class InterConverter implements PropertyConverter<ArrayList<Integer>, String> {
    private final Gson mGson = new Gson();

    @Override // org.greenrobot.greendao.converter.PropertyConverter
    public ArrayList<Integer> convertToEntityProperty(String str) {
        return (ArrayList) this.mGson.fromJson(str, new TypeToken<ArrayList<Integer>>() { // from class: cn.com.heaton.shiningmask.dao.InterConverter.1
        }.getType());
    }

    @Override // org.greenrobot.greendao.converter.PropertyConverter
    public String convertToDatabaseValue(ArrayList<Integer> arrayList) {
        return this.mGson.toJson(arrayList);
    }
}