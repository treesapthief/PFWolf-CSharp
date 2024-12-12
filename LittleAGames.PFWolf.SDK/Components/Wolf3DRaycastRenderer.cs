﻿namespace LittleAGames.PFWolf.SDK.Components;

public class Wolf3DRaycastRenderer : Renderer
{
    private readonly Map _map;
    
    public Wolf3DRaycastRenderer(Camera camera, Map map, int width, int height) : base(camera, width, height)
    {
        _map = map;
        _result = new byte[Width, Height];

        BuildTables();

        CalcProjection(0x5700L);

    }

    private const int ANGLES = 360;             // must be divisable by 4
    private const int ANGLEQUAD = (ANGLES / 4);
    private const int FINEANGLES = 3600;
    private const int ANG90 = (FINEANGLES / 4);
    private const int ANG180 = (ANG90 * 2);
    private const int ANG270 = (ANG90 * 3);
    private const int ANG360 = (ANG90 * 4);
    private const int VANG90 = (ANGLES / 4);
    private const int VANG180 = (VANG90 * 2);
    private const int VANG270 = (VANG90 * 3);
    private const int VANG360 = (VANG90 * 4);

    private const long TILESHIFT = 16L;

    private const int TEXTURESHIFT = 6;

    private const int FIXED2TEXSHIFT = 4;
    private const int TEXTURESIZE = (1 << TEXTURESHIFT);
    private readonly int TEXTUREMASK = (TEXTURESIZE * (TEXTURESIZE - 1));

    private long MINDIST = 0x5800L;
    private long VIEWGLOBAL = 0x10000;
    private const long GLOBAL1 = (1L << 16);
    private readonly long TILEGLOBAL = GLOBAL1;
    private int focalLength;
    private int scale;
    private int heightNumerator;

    private int[] finetangent = new int[FINEANGLES / 4];

    private short[] pixelAngle = new short[640]; // screenWidth
    private short[] wallHeight = new short[640]; // screenWidth
    
    
    private ushort    tilehit;
    private int     pixx;

    private short   xtile,ytile;
    private short   xtilestep = 0,ytilestep = 0;
    private int   xintercept,yintercept;
    private int   xinttile,yinttile;
    private ushort    texdelta;
    private ushort[]    horizwall = new ushort[90];
    private ushort[] vertwall = new ushort[90];
    
    private int viewSin;
    private int viewCos;
    private int viewX;
    private int viewY;

    
    private int     postx;

    private byte[,] _result;
    
    private void BuildTables()
    {
//
// calculate fine tangents
//

        int i;
        for(i=0;i<FINEANGLES/8;i++)
        {
            double tang=Math.Tan((i+0.5)/MathUtilities.RadiansToInteger);
            finetangent[i]=(int)(tang*GLOBAL1);
            finetangent[FINEANGLES/4-1-i]=(int)((1/tang)*GLOBAL1);
        }
    }
    
    private void CalcProjection(long focal)
    {
        int     i;
        int    intang;
        float   angle;
        double  tang;
        int     halfview;
        double  facedist;

        focalLength = (int)focal;
        facedist = focal+MINDIST;
        halfview = Width/2;                                 // half view in pixels

        //
        // calculate scale value for vertical height calculations
        // and sprite x calculations
        //
        scale = (int) (halfview*facedist/(VIEWGLOBAL/2));

        //
        // divide heightnumerator by a posts distance to get the posts height for
        // the heightbuffer.  The pixel height is height>>2
        //
        heightNumerator = (int)((TILEGLOBAL*scale)>>6);

        //
        // calculate the angle offset from view angle of each pixel's ray
        //

        for (i=0;i<halfview;i++)
        {
            // start 1/2 pixel over, so viewangle bisects two middle pixels
            tang = (int)i*VIEWGLOBAL/Width/facedist;
            angle = (float) Math.Atan(tang);
            intang = (int) (angle*MathUtilities.RadiansToInteger);
            pixelAngle[halfview-1-i] = (short)intang;
            pixelAngle[halfview+i] = (short)(-intang);
        }
    }

    public static Wolf3DRaycastRenderer Create(Camera camera, Map map, int width, int height)
        => new(camera, map, width, height);
    
    public override byte[,] Render()
    {
        _result.Fill((byte)0x19);

        var focalLength = 0x5700;

        var viewAngle = Camera.Angle;

        var midAngle = viewAngle * (FINEANGLES / ANGLES);

        viewSin = (int)Math.Sin(viewAngle) << (int)TILESHIFT;
        viewCos = (int)Math.Cos(viewAngle) << (int)TILESHIFT;

        viewX = Camera.X - FixedMul(focalLength, viewCos);
        viewY = Camera.Y + FixedMul(focalLength, viewSin);

        var focaltx = (short)(viewX>> (int)TILESHIFT);
        var focalty = (short)(viewY>> (int)TILESHIFT);
        
        // These are where the player is in a partial tile
        var xpartialdown = viewX&(TILEGLOBAL-1);
        var xpartialup = xpartialdown ^ (TILEGLOBAL - 1);
        var ypartialdown = viewY&(TILEGLOBAL-1);
        var ypartialup = ypartialdown ^ (TILEGLOBAL - 1);

        short   angle;
        int   xstep = 0,ystep=0;
        int     xinttemp,yinttemp;                            // holds temporary intercept position
        uint  xpartial = 0,ypartial = 0;
        
        for (pixx = 0; pixx < Width; pixx++)
        {
            angle = (short)(midAngle + pixelAngle[pixx]);
            
            if (angle < 0)                                      // -90 - -1 degree arc
                angle += ANG360;                                // -90 is the same as 270
            if (angle >= ANG360)                                // 360-449 degree arc
                angle -= ANG360;                                // -449 is the same as 89

            //
            // setup xstep/ystep based on angle
            //
            if (angle < ANG90)                                  // 0-89 degree arc
            {
                xtilestep = 1;
                ytilestep = -1;
                xstep = finetangent[ANG90 - 1 - angle];
                ystep = -finetangent[angle];
                xpartial = (uint)xpartialup;
                ypartial = (uint)ypartialdown;
            }
            else if (angle < ANG180)                            // 90-179 degree arc
            {
                xtilestep = -1;
                ytilestep = -1;
                xstep = -finetangent[angle - ANG90];
                ystep = -finetangent[ANG180 - 1 - angle];
                xpartial = (uint)xpartialdown;
                ypartial = (uint)ypartialdown;
            }
            else if (angle < ANG270)                            // 180-269 degree arc
            {
                xtilestep = -1;
                ytilestep = 1;
                xstep = -finetangent[ANG270 - 1 - angle];
                ystep = finetangent[angle - ANG180];
                xpartial = (uint)xpartialdown;
                ypartial = (uint)ypartialup;
            }
            else if (angle < ANG360)                            // 270-359 degree arc
            {
                xtilestep = 1;
                ytilestep = 1;
                xstep = finetangent[angle - ANG270];
                ystep = finetangent[ANG360 - 1 - angle];
                xpartial = (uint)xpartialup;
                ypartial = (uint)ypartialup;
            }
            
            //
            // initialise variables for intersection testing
            //
            yintercept = FixedMul(ystep,(int)xpartial) + viewY;
            yinttile = yintercept >> (int)TILESHIFT;
            xtile = (short)(focaltx + xtilestep);

            xintercept = FixedMul(xstep,(int)ypartial) + viewX;
            xinttile = xintercept >> (int)TILESHIFT;
            ytile = (short)(focalty + ytilestep);

            texdelta = 0;
            
//
// trace along this angle until we hit a wall
//
// CORE LOOP!
//
            while (true)
            {
                //
                // check intersections with vertical walls
                //
                if ((xtile - xtilestep) == xinttile && (ytile - ytilestep) == yinttile)
                    yinttile = ytile;

                if ((ytilestep == -1 && yinttile <= ytile) || (ytilestep == 1 && yinttile >= ytile))
                {
                    var wall = _map.FindWall(xinttile, ytile);// tilemap[xinttile][ytile]; // TODO: Get wall?
                    if (wall != null)
                    {
                        yintercept = ytile << (int)TILESHIFT;

                        HitHorizWall();
                    }
                }
                
                //
                // mark the tile as visible and setup for next step
                //
                //spotvis[xtile][yinttile] = true;
                xtile += xtilestep;
                yintercept += ystep;
                yinttile = yintercept >> (int)TILESHIFT;
            }

            continue;
            while (true)
            {
                //
                // check intersections with horizontal walls
                //
                if ((xtile - xtilestep) == xinttile && (ytile - ytilestep) == yinttile)
                    xinttile = xtile;
            }
        }
        
        return _result;
    }

    private void HitHorizWall()
    {
        int wallpic;
        int texture;

        texture = ((xintercept - texdelta) >> FIXED2TEXSHIFT) & TEXTUREMASK;

        if (ytilestep == -1)
            yintercept += (int)TILEGLOBAL;
        else
            texture = TEXTUREMASK - texture;

        wallHeight[pixx] = CalcHeight();
        postx = pixx;

        // if (tilehit & BIT_WALL)
        // {
        //     //
        //     // check for adjacent doors
        //     //
        //     if (tilemap[xinttile][ytile - ytilestep] & BIT_DOOR)
        //         wallpic = DOORWALL + 2;
        //     else
        //         wallpic = horizwall[tilehit & ~BIT_WALL];
        // }
        // else
            wallpic = horizwall[tilehit];

            // texture is the offset in the byte array
            //var postsource = 0 + texture;//PM_GetPage(wallpic) + texture;
            var wall = new byte[64];
        ScalePost (wall);
    }

    private void ScalePost(byte[] wall)
    {
        var centerY = Height / 2;
        var bufferPitch = 640;
        int ywcount, yoffs, yw, yd, yendoffs;
        byte col;

        ywcount = yd = wallHeight[postx] >> 3;
        if(yd <= 0) yd = 100;

        yoffs = (centerY - ywcount) * bufferPitch;
        if(yoffs < 0) yoffs = 0;
        yoffs += postx;

        yendoffs = centerY + ywcount - 1;
        yw=TEXTURESIZE-1;

        while(yendoffs >= 400)
        {
            ywcount -= TEXTURESIZE/2;
            while(ywcount <= 0)
            {
                ywcount += yd;
                yw--;
            }
            yendoffs--;
        }
        if(yw < 0) return;

        col = wall[yw];
        yendoffs = yendoffs * bufferPitch + postx;
        while(yoffs <= yendoffs)
        {
            _result[yendoffs/640, yendoffs*640] = col;
            ywcount -= TEXTURESIZE/2;
            if(ywcount <= 0)
            {
                do
                {
                    ywcount += yd;
                    yw--;
                }
                while(ywcount <= 0);
                if(yw < 0) break;
                col = wall[yw];
            }
            yendoffs -= bufferPitch;
        }
    }

    private int FixedMul(int a, int b)
        => (int)(((long)a*b+0x8000) >> 16);
    
    
    private static void DrawLine(byte[,] grid, int startX, int startY, double angle, int length, byte color)
    {
        int x = startX;
        int y = startY;

        // Calculate the direction in x and y using the angle
        double dx = Math.Cos(angle);
        double dy = -Math.Sin(angle);

        for (int i = 0; i < length; i++)
        {
            // Plot the point in the 2D array (make sure it's within bounds)
            if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
            {
                grid[x, y] = color;
            }

            // Update x and y for the next point
            x = (int)(startX + i * dx);
            y = (int)(startY + i * dy);
        }
    }
    
    private short CalcHeight ()
    {
        short height;
        int   gx,gy,gxt,gyt,nx;

//
// translate point to view centered coordinates
//
        gx = xintercept - viewX;
        gy = yintercept - viewY;

//
// calculate nx
//
        gxt = FixedMul(gx,viewCos);
        gyt = FixedMul(gy,viewSin);
        nx = gxt - gyt;

//
// calculate perspective ratio
//
        if (nx < MINDIST)
            nx = (int)MINDIST;             // don't let divide overflow

        height = (short)(heightNumerator / (nx >> 8));

        return height;
    }
}