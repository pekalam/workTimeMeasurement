﻿using AutoMapper;
using Infrastructure.WorkTimeAlg;
using OpenCvSharp;

namespace Infrastructure.Db
{
    internal class DbTestImageProfile : Profile
    {
        public DbTestImageProfile()
        {
            CreateMap<TestImage, DbTestImage>()
                .ForMember(db => db.Img, opt => { opt.MapFrom<byte[]>((testImage, dbEnt) => testImage.Img.ToBytes()); })
                .ForMember(db => db.FaceEncoding,
                    opt =>
                    {
                        opt.MapFrom<byte[]>((testImage, _) =>
                            FaceEncodingHelpers.Serialize(testImage.FaceEncoding.Value));
                    })
                .ForMember(db => db.FaceLocation_x, opt => opt.MapFrom<int>(image => image.FaceLocation.X))
                .ForMember(db => db.FaceLocation_x, opt => opt.MapFrom<int>(image => image.FaceLocation.Y))
                .ForMember(db => db.FaceLocation_right, opt => opt.MapFrom<int>(image => image.FaceLocation.Right))
                .ForMember(db => db.FaceLocation_bottom, opt => opt.MapFrom<int>(image => image.FaceLocation.Bottom));


            CreateMap<DbTestImage, TestImage>()
                .ForMember(image => image.FaceEncoding, opt =>
                {
                    opt.MapFrom<FaceEncodingData>((db, _) =>
                    {
                        var encoding = FaceEncodingHelpers.Deserialize(db.FaceEncoding);
                        return new FaceEncodingData(encoding);
                    });
                })
                .ForMember(image => image.Img, opt =>
                {
                    opt.MapFrom<Mat>((db, _) =>
                    {
                        var mat = Mat.FromImageData(db.Img);
                        return mat;
                    });
                })
                .ForMember(image => image.FaceLocation,
                    opt =>
                    {
                        opt.MapFrom<Rect>((db, _) => new Rect(db.FaceLocation_x, db.FaceLocation_y,
                            db.FaceLocation_right, db.FaceLocation_bottom));
                    });
        }
    }
}