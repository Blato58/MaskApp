package cn.com.heaton.shiningmask.dao;

import android.database.Cursor;
import android.database.sqlite.SQLiteStatement;
import cn.com.heaton.shiningmask.dao.bean.CropImage;
import org.greenrobot.greendao.AbstractDao;
import org.greenrobot.greendao.Property;
import org.greenrobot.greendao.database.Database;
import org.greenrobot.greendao.database.DatabaseStatement;
import org.greenrobot.greendao.internal.DaoConfig;

/* JADX INFO: loaded from: classes.dex */
public class CropImageDao extends AbstractDao<CropImage, Long> {
    public static final String TABLENAME = "CROP_IMAGE";

    public static class Properties {
        public static final Property Id = new Property(0, Long.class, "id", true, "_id");
        public static final Property ImageUrl = new Property(1, String.class, "imageUrl", false, "IMAGE_URL");
        public static final Property SelectStatus = new Property(2, Boolean.TYPE, "selectStatus", false, "SELECT_STATUS");
        public static final Property ImageData = new Property(3, byte[].class, "imageData", false, "IMAGE_DATA");
        public static final Property Time = new Property(4, byte[].class, "time", false, "TIME");
        public static final Property Index = new Property(5, Integer.TYPE, "index", false, "INDEX");
        public static final Property TimeInt = new Property(6, Integer.TYPE, "timeInt", false, "TIME_INT");
        public static final Property ImageIndex = new Property(7, Integer.TYPE, "imageIndex", false, "IMAGE_INDEX");
    }

    @Override // org.greenrobot.greendao.AbstractDao
    protected final boolean isEntityUpdateable() {
        return true;
    }

    public CropImageDao(DaoConfig daoConfig) {
        super(daoConfig);
    }

    public CropImageDao(DaoConfig daoConfig, DaoSession daoSession) {
        super(daoConfig, daoSession);
    }

    public static void createTable(Database database, boolean z) {
        database.execSQL("CREATE TABLE " + (z ? "IF NOT EXISTS " : "") + "\"CROP_IMAGE\" (\"_id\" INTEGER PRIMARY KEY AUTOINCREMENT ,\"IMAGE_URL\" TEXT,\"SELECT_STATUS\" INTEGER NOT NULL ,\"IMAGE_DATA\" BLOB,\"TIME\" BLOB,\"INDEX\" INTEGER NOT NULL ,\"TIME_INT\" INTEGER NOT NULL ,\"IMAGE_INDEX\" INTEGER NOT NULL );");
    }

    public static void dropTable(Database database, boolean z) {
        database.execSQL("DROP TABLE " + (z ? "IF EXISTS " : "") + "\"CROP_IMAGE\"");
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(DatabaseStatement databaseStatement, CropImage cropImage) {
        databaseStatement.clearBindings();
        Long id = cropImage.getId();
        if (id != null) {
            databaseStatement.bindLong(1, id.longValue());
        }
        String imageUrl = cropImage.getImageUrl();
        if (imageUrl != null) {
            databaseStatement.bindString(2, imageUrl);
        }
        databaseStatement.bindLong(3, cropImage.getSelectStatus() ? 1L : 0L);
        byte[] imageData = cropImage.getImageData();
        if (imageData != null) {
            databaseStatement.bindBlob(4, imageData);
        }
        byte[] time = cropImage.getTime();
        if (time != null) {
            databaseStatement.bindBlob(5, time);
        }
        databaseStatement.bindLong(6, cropImage.getIndex());
        databaseStatement.bindLong(7, cropImage.getTimeInt());
        databaseStatement.bindLong(8, cropImage.getImageIndex());
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(SQLiteStatement sQLiteStatement, CropImage cropImage) {
        sQLiteStatement.clearBindings();
        Long id = cropImage.getId();
        if (id != null) {
            sQLiteStatement.bindLong(1, id.longValue());
        }
        String imageUrl = cropImage.getImageUrl();
        if (imageUrl != null) {
            sQLiteStatement.bindString(2, imageUrl);
        }
        sQLiteStatement.bindLong(3, cropImage.getSelectStatus() ? 1L : 0L);
        byte[] imageData = cropImage.getImageData();
        if (imageData != null) {
            sQLiteStatement.bindBlob(4, imageData);
        }
        byte[] time = cropImage.getTime();
        if (time != null) {
            sQLiteStatement.bindBlob(5, time);
        }
        sQLiteStatement.bindLong(6, cropImage.getIndex());
        sQLiteStatement.bindLong(7, cropImage.getTimeInt());
        sQLiteStatement.bindLong(8, cropImage.getImageIndex());
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // org.greenrobot.greendao.AbstractDao
    public Long readKey(Cursor cursor, int i) {
        if (cursor.isNull(i)) {
            return null;
        }
        return Long.valueOf(cursor.getLong(i));
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // org.greenrobot.greendao.AbstractDao
    public CropImage readEntity(Cursor cursor, int i) {
        int i2 = i + 1;
        int i3 = i + 3;
        int i4 = i + 4;
        return new CropImage(cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i)), cursor.isNull(i2) ? null : cursor.getString(i2), cursor.getShort(i + 2) != 0, cursor.isNull(i3) ? null : cursor.getBlob(i3), cursor.isNull(i4) ? null : cursor.getBlob(i4), cursor.getInt(i + 5), cursor.getInt(i + 6), cursor.getInt(i + 7));
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public void readEntity(Cursor cursor, CropImage cropImage, int i) {
        cropImage.setId(cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i)));
        int i2 = i + 1;
        cropImage.setImageUrl(cursor.isNull(i2) ? null : cursor.getString(i2));
        cropImage.setSelectStatus(cursor.getShort(i + 2) != 0);
        int i3 = i + 3;
        cropImage.setImageData(cursor.isNull(i3) ? null : cursor.getBlob(i3));
        int i4 = i + 4;
        cropImage.setTime(cursor.isNull(i4) ? null : cursor.getBlob(i4));
        cropImage.setIndex(cursor.getInt(i + 5));
        cropImage.setTimeInt(cursor.getInt(i + 6));
        cropImage.setImageIndex(cursor.getInt(i + 7));
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final Long updateKeyAfterInsert(CropImage cropImage, long j) {
        cropImage.setId(Long.valueOf(j));
        return Long.valueOf(j);
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public Long getKey(CropImage cropImage) {
        if (cropImage != null) {
            return cropImage.getId();
        }
        return null;
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public boolean hasKey(CropImage cropImage) {
        return cropImage.getId() != null;
    }
}