﻿namespace App
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    public enum TypeOfDicomFile
    {
        NotDicom,
        Dicom3File,
        DicomOldTypeFile,
        DicomUnknownTransferSyntax
    }

    public class DicomDecoder
    {
        const uint PIXEL_REPRESENTATION = 0x00280103;
        const uint TRANSFER_SYNTAX_UID = 0x00020010;
        const uint SERIES_NUMBER = 0x00200011;
        const uint INSTANCE_NUMBER = 0x00200013;
        const uint MODALITY = 0x00080060;
        const uint SLICE_THICKNESS = 0x00180050;
        const uint SLICE_SPACING = 0x00180088;
        const uint SAMPLES_PER_PIXEL = 0x00280002;
        const uint PHOTOMETRIC_INTERPRETATION = 0x00280004;
        const uint PLANAR_CONFIGURATION = 0x00280006;
        const uint NUMBER_OF_FRAMES = 0x00280008;
        const uint ROWS = 0x00280010;
        const uint COLUMNS = 0x00280011;
        const uint PIXEL_SPACING = 0x00280030;
        const uint BITS_ALLOCATED = 0x00280100;
        const uint WINDOW_CENTER = 0x00281050;
        const uint WINDOW_WIDTH = 0x00281051;
        const uint RESCALE_INTERCEPT = 0x00281052;
        const uint RESCALE_SLOPE = 0x00281053;
        const uint RED_PALETTE = 0x00281201;
        const uint GREEN_PALETTE = 0x00281202;
        const uint BLUE_PALETTE = 0x00281203;
        const uint ICON_IMAGE_SEQUENCE = 0x00880200;
        const uint PIXEL_DATA = 0x7FE00010;

        const string ITEM = "FFFEE000";
        const string ITEM_DELIMITATION = "FFFEE00D";
        const string SEQUENCE_DELIMITATION = "FFFEE0DD";

        const int
            AE = 0x4145,
            AS = 0x4153,
            AT = 0x4154,
            CS = 0x4353,
            DA = 0x4441,
            DS = 0x4453,
            DT = 0x4454,
            FD = 0x4644,
            FL = 0x464C,
            IS = 0x4953,
            LO = 0x4C4F,
            LT = 0x4C54,
            PN = 0x504E,
            SH = 0x5348,
            SL = 0x534C,
            SS = 0x5353,
            ST = 0x5354,
            TM = 0x544D,
            UI = 0x5549,
            UL = 0x554C,
            US = 0x5553,
            UT = 0x5554,
            OB = 0x4F42,
            OW = 0x4F57,
            SQ = 0x5351,
            UN = 0x554E,
            QQ = 0x3F3F,
            RT = 0x5254;

        const int ID_OFFSET = 128; //location of "DICM"
        const int IMPLICIT_VR = 0x2D2D; // '--' 
        const String DICM = "DICM";

        public int bitsAllocated;
        public int width;
        public int height;
        public int offset;
        public int nImages;
        public int samplesPerPixel;
        public string[] PixelSpacingValue;
        public double pixeSpace = 1.0;
        public double pixelDepth = 1.0;
        public double pixelWidth = 1.0;
        public double pixelHeight = 1.0;
        public string unit;
        public double windowCentre, windowWidth;
        public bool signedImage;
        public TypeOfDicomFile typeofDicomFile;
        public List<string> dicomInfo;
        public bool dicmFound; // "DICM" found at offset 128

        DicomDictionary dic;
        BinaryReader file;
        String dicomFileName;
        String photoInterpretation;
        bool littleEndian = true;
        bool oddLocations; // one or more tags at odd locations
        bool bigEndianTransferSyntax;
        bool inSequence;
        bool widthTagFound;
        bool heightTagFound;
        bool pixelDataTagFound;
        int location;
        int elementLength;
        int vr; // Value Representation
        int min16 = short.MinValue;
        int max16 = ushort.MaxValue;
        int pixelRepresentation;
        double rescaleIntercept;
        double rescaleSlope;
        byte[] reds;
        byte[] greens;
        byte[] blues;
        List<int> pixels16Int;


        public DicomDecoder()
        {
            dic = new DicomDictionary();
            signedImage = false;
            dicomInfo = new List<string>();
            InitializeDicom();
        }

        void InitializeDicom()
        {
            bitsAllocated = 0;
            width = 1;
            height = 1;
            offset = 1;
            nImages = 1;
            samplesPerPixel = 1;
            photoInterpretation = "";
            unit = "mm";
            windowCentre = 0;
            windowWidth = 0;
            signedImage = false;
            widthTagFound = false;
            heightTagFound = false;
            pixelDataTagFound = false;
            rescaleIntercept = 0.0; // Default value
            rescaleSlope = 1.0; // Default value
            typeofDicomFile = TypeOfDicomFile.NotDicom;
        }

        public string DicomFileName
        {
            set
            {
                dicomFileName = value;
                InitializeDicom();

                // Thanks to CodeProject member Alphons van der Heijden for 
                //   suggesting to add this - FileAccess.Read  (July 2010)
                file = new BinaryReader(File.Open(dicomFileName, FileMode.Open, FileAccess.Read));
                location = 0; // Reset the location
                dicomInfo.Clear();
                try
                {
                    bool readResult = ReadFileInfo();
                    if (readResult && widthTagFound && heightTagFound && pixelDataTagFound)
                    {
                        ReadPixels();
                        typeofDicomFile = dicmFound ? TypeOfDicomFile.Dicom3File : TypeOfDicomFile.DicomOldTypeFile;
                    }
                }
                catch
                {
                    // Nothing here
                }
                finally
                {
                    file.Close();
                }
            }
            get { return dicomFileName; }
        }

        public int SeriesNumber { get; set; }

        public int InstanceNumber { get; set; }

        public List<ushort> Pixels16Origin { get; set; }

        public List<ushort> Pixels16 { get; set; }

        string GetString(int length)
        {
            byte[] buf = new byte[length];
            file.BaseStream.Position = location;
            file.Read(buf, 0, length);
            location += length;
            string s = Encoding.ASCII.GetString(buf);
            return s;
        }

        byte GetByte() // Changed return type to byte
        {
            file.BaseStream.Position = location;
            byte b = file.ReadByte();
            ++location;
            return b;
        }

        ushort GetShort() // Changed return type to ushort
        {
            byte b0 = GetByte();
            byte b1 = GetByte();
            ushort s;
            s = littleEndian ? Convert.ToUInt16((b1 << 8) + b0) : Convert.ToUInt16((b0 << 8) + b1);
            return s;
        }

        int GetInt()
        {
            byte b0 = GetByte();
            byte b1 = GetByte();
            byte b2 = GetByte();
            byte b3 = GetByte();
            int i;
            if (littleEndian)
                i = (b3 << 24) + (b2 << 16) + (b1 << 8) + b0;
            else
                i = (b0 << 24) + (b1 << 16) + (b2 << 8) + b3;
            return i;
        }

        byte[] GetLut(int length)
        {
            if ((length & 1) != 0) // odd
            {
                GetString(length);
                return null;
            }

            length /= 2;
            byte[] lut = new byte[length];
            for (int i = 0; i < length; ++i)
                lut[i] = Convert.ToByte(GetShort() >> 8);
            return lut;
        }

        int GetLength()
        {
            byte b0 = GetByte();
            byte b1 = GetByte();
            byte b2 = GetByte();
            byte b3 = GetByte();

            // Cannot know whether the VR is implicit or explicit without the 
            // complete Dicom Data Dictionary. 
            vr = (b0 << 8) + b1;

            switch (vr)
            {
                case OB:
                case OW:
                case SQ:
                case UN:
                case UT:
                    // Explicit VR with 32-bit length if other two bytes are zero
                    if ((b2 == 0) || (b3 == 0)) return GetInt();
                    // Implicit VR with 32-bit length
                    vr = IMPLICIT_VR;
                    if (littleEndian)
                        return ((b3 << 24) + (b2 << 16) + (b1 << 8) + b0);
                    return ((b0 << 24) + (b1 << 16) + (b2 << 8) + b3);
                // break; // Not necessary
                case AE:
                case AS:
                case AT:
                case CS:
                case DA:
                case DS:
                case DT:
                case FD:
                case FL:
                case IS:
                case LO:
                case LT:
                case PN:
                case SH:
                case SL:
                case SS:
                case ST:
                case TM:
                case UI:
                case UL:
                case US:
                case QQ:
                case RT:
                    // Explicit vr with 16-bit length
                    if (littleEndian)
                        return ((b3 << 8) + b2);
                    return ((b2 << 8) + b3);
                default:
                    // Implicit VR with 32-bit length...
                    vr = IMPLICIT_VR;
                    if (littleEndian)
                        return ((b3 << 24) + (b2 << 16) + (b1 << 8) + b0);
                    return ((b0 << 24) + (b1 << 16) + (b2 << 8) + b3);
            }
        }

        int GetNextTag()
        {
            int groupWord = GetShort();
            if (groupWord == 0x0800 && bigEndianTransferSyntax)
            {
                littleEndian = false;
                groupWord = 0x0008;
            }
            int elementWord = GetShort();
            int tag = groupWord << 16 | elementWord;

            elementLength = GetLength();

            // Hack to read some GE files
            if (elementLength == 13 && !oddLocations)
                elementLength = 10;

            // "Undefined" element length.
            // This is a sort of bracket that encloses a sequence of elements.
            if (elementLength == -1)
            {
                elementLength = 0;
                inSequence = true;
            }
            return tag;
        }

        String GetHeaderInfo(int tag, String value)
        {
            string str = tag.ToString("X8");
            if (str == ITEM_DELIMITATION || str == SEQUENCE_DELIMITATION)
            {
                inSequence = false;
                return null;
            }

            string id = null;

            if (dic.Dict.ContainsKey(str))
            {
                id = dic.Dict[str];
                if (id != null)
                {
                    if (vr == IMPLICIT_VR)
                        vr = (id[0] << 8) + id[1];
                    id = id.Substring(2);
                }
            }

            if (str == ITEM)
                return (id ?? ":null");
            if (value != null)
                return id + ": " + value;

            switch (vr)
            {
                case FD:
                    for (int i = 0; i < elementLength; ++i)
                        GetByte();
                    break;
                case FL:
                    for (int i = 0; i < elementLength; i++)
                        GetByte();
                    break;
                case AE:
                case AS:
                case AT:
                case CS:
                case DA:
                case DS:
                case DT:
                case IS:
                case LO:
                case LT:
                case PN:
                case SH:
                case ST:
                case TM:
                case UI:
                    value = GetString(elementLength);
                    break;
                case US:
                    if (elementLength == 2)
                        value = Convert.ToString(GetShort());
                    else
                    {
                        value = "";
                        int n = elementLength/2;
                        for (int i = 0; i < n; i++)
                            value += Convert.ToString(GetShort()) + " ";
                    }
                    break;
                case IMPLICIT_VR:
                    value = GetString(elementLength);
                    if (elementLength > 44)
                        value = null;
                    break;
                case SQ:
                    value = "";
                    bool privateTag = ((tag >> 16) & 1) != 0;
                    if (tag != ICON_IMAGE_SEQUENCE && !privateTag)
                        break;
                    goto default;
                default:
                    location += elementLength;
                    value = "";
                    break;
            }

            if (value != null && id == null && value != "")
                return "---: " + value;
            if (id == null)
                return null;
            return id + ": " + value;
        }

        void AddInfo(int tag, string value)
        {
            string info = GetHeaderInfo(tag, value);

            string str = tag.ToString("X");
            string strPadded = str.PadLeft(8, '0');
            if (inSequence && info != null && vr != SQ)
                info = ">" + info;
            if (info != null && str != ITEM)
            {
                var strInfo = info.Contains("---") ? info.Replace("---", "Private Tag") : info;

                dicomInfo.Add(strPadded + "//" + strInfo);
            }
        }

        void AddInfo(int tag, int value)
        {
            AddInfo(tag, Convert.ToString(value));
        }

        void GetSpatialScale(String scale)
        {
            double xscale = 0, yscale = 0;
            int i = scale.IndexOf('\\');
            if (i == 1) // Aug 2012, Fixed an issue found while opening some images
            {
                yscale = Convert.ToDouble(scale.Substring(0, i), new CultureInfo("en-US"));
                xscale = Convert.ToDouble(scale.Substring(i + 1), new CultureInfo("en-US"));
            }
            if (xscale != 0.0 && yscale != 0.0)
            {
                pixelWidth = xscale;
                pixelHeight = yscale;
                unit = "mm";
            }
        }

        public bool ReadFileInfo()
        {
            long skipCount = Convert.ToInt32(ID_OFFSET);
            bitsAllocated = 16;

            file.BaseStream.Seek(skipCount, SeekOrigin.Begin);
            location += ID_OFFSET;

            if (GetString(4) != DICM)
            {
                // This is for reading older DICOM files (Before 3.0)
                // Seek from the beginning of the file
                file.BaseStream.Seek(0, SeekOrigin.Begin);
                location = 0;

                // Older DICOM files do not have the preamble and prefix
                dicmFound = false;

                // Continue reading further.
                // See whether the width, height and pixel data tags
                // are present. If these tags are present, then it we conclude that this is a 
                // DICOM file, because all DICOM files should have at least these three tags.
                // Otherwise, it is not a DICOM file.
            }
            else
            {
                // We have a DICOM 3.0 file
                dicmFound = true;
            }

            bool decodingTags = true;
            samplesPerPixel = 1;
            photoInterpretation = "";
            string modality;

            while (decodingTags)
            {
                int tag = GetNextTag();
                if ((location & 1) != 0)
                    oddLocations = true;

                if (inSequence)
                {
                    AddInfo(tag, null);
                    continue;
                }

                string s;
                switch (tag)
                {
                    case (int) (TRANSFER_SYNTAX_UID):
                        s = GetString(elementLength);
                        AddInfo(tag, s);
                        if (s.IndexOf("1.2.4", StringComparison.Ordinal) > -1 ||
                            s.IndexOf("1.2.5", StringComparison.Ordinal) > -1)
                        {
                            file.Close();
                            typeofDicomFile = TypeOfDicomFile.DicomUnknownTransferSyntax;
                            // Return gracefully indicating that this type of 
                            // Transfer Syntax cannot be handled
                            return false;
                        }
                        if (s.IndexOf("1.2.840.10008.1.2.2", StringComparison.Ordinal) >= 0)
                            bigEndianTransferSyntax = true;
                        break;
                    case (int) (SERIES_NUMBER):
                        s = GetString(elementLength);
                        SeriesNumber = Convert.ToInt32(s);
                        AddInfo(tag, s);
                        break;
                    case (int) (INSTANCE_NUMBER):
                        s = GetString(elementLength);
                        InstanceNumber = Convert.ToInt32(s);
                        ;
                        AddInfo(tag, s);
                        break;

                    case (int) MODALITY:
                        modality = GetString(elementLength);
                        AddInfo(tag, modality);
                        break;
                    case (int) (NUMBER_OF_FRAMES):
                        s = GetString(elementLength);
                        AddInfo(tag, s);
                        double frames = Convert.ToDouble(s, new CultureInfo("en-US"));
                        if (frames > 1.0)
                            nImages = (int) frames;
                        break;
                    case (int) (SAMPLES_PER_PIXEL):
                        samplesPerPixel = GetShort();
                        AddInfo(tag, samplesPerPixel);
                        break;
                    case (int) (PHOTOMETRIC_INTERPRETATION):
                        photoInterpretation = GetString(elementLength);
                        photoInterpretation = photoInterpretation.Trim();
                        AddInfo(tag, photoInterpretation);
                        break;
                    case (int) (PLANAR_CONFIGURATION):
                        int planarConfiguration = GetShort();
                        AddInfo(tag, planarConfiguration);
                        break;
                    case (int) (ROWS):
                        height = GetShort();
                        AddInfo(tag, height);
                        heightTagFound = true;
                        break;
                    case (int) (COLUMNS):
                        width = GetShort();
                        AddInfo(tag, width);
                        widthTagFound = true;
                        break;
                    case (int) (PIXEL_SPACING):
                        String scale = GetString(elementLength);
                        PixelSpacingValue = scale.Split(new char[] { '\\' },2);
                        GetSpatialScale(scale);
                        AddInfo(tag, scale);
                        
                        break;
                    case (int) (SLICE_SPACING):
                        String space = GetString(elementLength);
                        pixeSpace = Convert.ToDouble(space, new CultureInfo("en-US"));
                        AddInfo(tag, space);
                        break;
                    case (int) (SLICE_THICKNESS):
                        String spacing = GetString(elementLength);
                        pixelDepth = Convert.ToDouble(spacing, new CultureInfo("en-US"));
                        AddInfo(tag, spacing);
                        break;
                    case (int) (BITS_ALLOCATED):
                        bitsAllocated = GetShort();
                        AddInfo(tag, bitsAllocated);
                        break;
                    case (int) (PIXEL_REPRESENTATION):
                        pixelRepresentation = GetShort();
                        AddInfo(tag, pixelRepresentation);
                        break;
                    case (int) (WINDOW_CENTER):
                        String center = GetString(elementLength);
                        int index = center.IndexOf('\\');
                        if (index != -1) center = center.Substring(index + 1);
                        windowCentre = Convert.ToDouble(center, new CultureInfo("en-US"));
                        AddInfo(tag, center);
                        break;
                    case (int) (WINDOW_WIDTH):
                        String widthS = GetString(elementLength);
                        index = widthS.IndexOf('\\');
                        if (index != -1) widthS = widthS.Substring(index + 1);
                        windowWidth = Convert.ToDouble(widthS, new CultureInfo("en-US"));
                        AddInfo(tag, widthS);
                        break;
                    case (int) (RESCALE_INTERCEPT):
                        String intercept = GetString(elementLength);
                        rescaleIntercept = Convert.ToDouble(intercept, new CultureInfo("en-US"));
                        AddInfo(tag, intercept);
                        break;
                    case (int) (RESCALE_SLOPE):
                        String slop = GetString(elementLength);
                        rescaleSlope = Convert.ToDouble(slop, new CultureInfo("en-US"));
                        AddInfo(tag, slop);
                        break;
                    case (int) (RED_PALETTE):
                        reds = GetLut(elementLength);
                        AddInfo(tag, elementLength/2);
                        break;
                    case (int) (GREEN_PALETTE):
                        greens = GetLut(elementLength);
                        AddInfo(tag, elementLength/2);
                        break;
                    case (int) (BLUE_PALETTE):
                        blues = GetLut(elementLength);
                        AddInfo(tag, elementLength/2);
                        break;
                    case (int) (PIXEL_DATA):
                        if (elementLength != 0)
                        {
                            offset = location;
                            AddInfo(tag, location);
                            decodingTags = false;
                        }
                        else
                            AddInfo(tag, null);
                        pixelDataTagFound = true;
                        break;
                    default:
                        AddInfo(tag, null);
                        break;
                }
            }
            return true;
        }

        void ReadPixels()
        {
            if (samplesPerPixel == 1 && bitsAllocated == 16)
            {
                Pixels16Origin?.Clear();
                pixels16Int?.Clear();

                Pixels16Origin = new List<ushort>();
                pixels16Int = new List<int>();
                int numPixels = width*height;
                byte[] bufByte = new byte[numPixels*2];

                byte[] signedData = new byte[2];
                file.BaseStream.Position = offset;
                file.Read(bufByte, 0, numPixels*2);
                int i;

                for (i = 0; i < numPixels; ++i)
                {
                    var i1 = i*2;
                    var b0 = bufByte[i1];
                    var b1 = bufByte[i1 + 1];
                    var unsignedS = Convert.ToUInt16((b1 << 8) + b0);
                    int pixVal;
                    if (pixelRepresentation == 0) // Unsigned
                    {
                        pixVal = (int) (unsignedS*rescaleSlope + rescaleIntercept);
                        if (photoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    else // Pixel representation is 1, indicating a 2s complement image
                    {
                        signedData[0] = b0;
                        signedData[1] = b1;
                        short sVal = BitConverter.ToInt16(signedData, 0);

                        // Need to consider rescale slope and intercepts to compute the final pixel value
                        pixVal = (int) (sVal*rescaleSlope + rescaleIntercept);
                        if (photoInterpretation == "MONOCHROME1")
                            pixVal = max16 - pixVal;
                    }
                    pixels16Int.Add(pixVal);
                }

                int minPixVal = pixels16Int.Min();
                signedImage = minPixVal < 0;

                // Use the above pixel data to populate the list pixels16 
                foreach (int pixel in pixels16Int)
                {
                    // We internally convert all 16-bit images to the range 0 - 65535
                    if (signedImage)
                        Pixels16Origin.Add((ushort) (pixel - min16));
                    else
                        Pixels16Origin.Add((ushort) (pixel));
                }

                pixels16Int.Clear();
            }

        }
    }
}
