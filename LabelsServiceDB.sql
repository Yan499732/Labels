PGDMP     	                    y            LabelsServiceDB    13.2    13.2 	    ?           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false            ?           0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false            ?           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false            ?           1262    16394    LabelsServiceDB    DATABASE     n   CREATE DATABASE "LabelsServiceDB" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE = 'Russian_Russia.1251';
 !   DROP DATABASE "LabelsServiceDB";
                postgres    false            ?            1259    16397    ServiceResults    TABLE     ?   CREATE TABLE public."ServiceResults" (
    "ID" integer NOT NULL,
    "FileOld" bytea,
    "FileNew" bytea,
    "LabelsNames" character varying(1000)[],
    "LabelsValues" character varying(1000)[],
    "Images" bytea,
    "WorkingTime" interval
);
 $   DROP TABLE public."ServiceResults";
       public         heap    postgres    false            ?            1259    16395    ServiceResults_ID_seq    SEQUENCE     ?   ALTER TABLE public."ServiceResults" ALTER COLUMN "ID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."ServiceResults_ID_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
            public          postgres    false    201            ?          0    16397    ServiceResults 
   TABLE DATA           ~   COPY public."ServiceResults" ("ID", "FileOld", "FileNew", "LabelsNames", "LabelsValues", "Images", "WorkingTime") FROM stdin;
    public          postgres    false    201   ?	       ?           0    0    ServiceResults_ID_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('public."ServiceResults_ID_seq"', 183, true);
          public          postgres    false    200            $           2606    16404 "   ServiceResults ServiceResults_pkey 
   CONSTRAINT     f   ALTER TABLE ONLY public."ServiceResults"
    ADD CONSTRAINT "ServiceResults_pkey" PRIMARY KEY ("ID");
 P   ALTER TABLE ONLY public."ServiceResults" DROP CONSTRAINT "ServiceResults_pkey";
       public            postgres    false    201            ?   8  x????j?0 ?s?{??g??C?	rI?v?i???'??:?{?a ?pb??|?AAͧ??W×:??4M??<?5%cMF?iTѪE7???馫^???|?"??c???{[?G?{?\a?X?=?cu?B????l????cĎ??k????V???Z??n?+??kk4f?2?Au?x?ّ0???u?{*	?Ԝ????O?D̥!q u? k;rD?<BRGJ??EB?v?8?R???Ԑ,???ą?Lz?F?;???#E??Ʉ???Qu?Bbh?d??{G????#(n@Qk?D:*1/"s????E?2L?0ߕȜa     