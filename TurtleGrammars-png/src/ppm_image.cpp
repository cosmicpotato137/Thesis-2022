#include "ppm_image.h"
#include <cassert>
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "stb/stb_image_write.h"

using namespace glm;
using namespace std;

Pixel Pixel::black = Pixel{ 0, 0, 0 };
Pixel Pixel::white = Pixel{ 255, 255, 255 };

Image::Image() : m_Data(0), m_Width(0), m_Height(0)
{
}

Image::Image(unsigned int width, unsigned int height) : m_Width(width), m_Height(height), m_Data()
{
    unsigned int s = width * height;
    m_Data = new Pixel[s];

	for (int i = 0; i < s; i++)
		m_Data[i] = Pixel::black;
}

//Image::Image(const Image& orig)
//{
//    cout << "not implemented\n";
//}

Image& Image::operator=(const Image& orig)
{
    if (&orig == this)
    {
        return *this;
    }

    cout << "not implemented\n";
    return *this;
}

Image::~Image()
{
    delete[] m_Data;
}

bool Image::save(const std::string& filename) const
{
    int result = stbi_write_png(filename.c_str(), m_Width, m_Height, 
        3, (unsigned char*) m_Data, m_Width*3);
    return (result == 1);
}

Pixel Image::get(int row, int col) const
{
    assert(row >= 0 && row < m_Height);
    assert(col >= 0 && col < m_Width);
    return m_Data[row*m_Width + col];
}

void Image::set(int row, int col, const Pixel& color)
{
    assert(row >= 0 && row < m_Height);
    assert(col >= 0 && col < m_Width);
    m_Data[row*m_Width + col] = color;
}

//
// line drawing algorithm -------------------
//

void draw_line_low(vec2 v0, vec2 v1, Image& img)
{
	int dx = v1.x - v0.x;
	int dy = v1.y - v0.y;

	// 'slope' of the color
	Pixel c0 = Pixel::white;
	Pixel c1 = Pixel::white;
	float dc[3] = { 0, 0, 0 };
	if (dx + dy)
	{
		dc[0] = (c1.r - c0.r) / (dx + dy);
		dc[1] = (c1.g - c0.g) / (dx + dy);
		dc[2] = (c1.b - c0.b) / (dx + dy);
	}
	int yi = 1;
	if (dy < 0)
	{
		yi = -1;
		dy = -dy;
	}

	int D = (2 * dy) - dx;
	int y = v0.y;

	for (int x = v0.x; x <= v1.x; x++)
	{
		uint8_t r = float(dc[0] * (x - v0.x + y - v0.y) + c0.r);
		uint8_t g = float(dc[1] * (x - v0.x + y - v0.y) + c0.g);
		uint8_t b = float(dc[2] * (x - v0.x + y - v0.y) + c0.b);
		if (x >= 0 && x < img.width() && y >= 0 && y < img.height())
			img.set(x, y, { r, g, b });
		if (D > 0)
		{
			y = y + yi;
			D = D + (2 * (dy - dx));
		}
		else
			D = D + 2 * dy;
	}
}

void draw_line_high(vec2 v0, vec2 v1, Image& img)
{
	int dx = v1.x - v0.x;
	int dy = v1.y - v0.y;

	// 'slope' of the color
	Pixel c0 = Pixel::white;
	Pixel c1 = Pixel::white;

	float dc[3] = { 0.0f, 0.0f, 0.0f };
	if (dx + dy)
	{
		dc[0] = (float)(c1.r - c0.r) / (dx + dy);
		dc[1] = (float)(c1.g - c0.g) / (dx + dy);
		dc[2] = (float)(c1.b - c0.b) / (dx + dy);
	}

	int xi = 1;
	if (dx < 0)
	{
		xi = -1;
		dx = -dx;
	}
	int D = (2 * dx) - dy;
	int x = v0.x;

	for (int y = v0.y; y <= v1.y; y++)
	{
		uint8_t r = float(dc[0] * (x - v0.x + y - v0.y) + c0.r);
		uint8_t g = float(dc[1] * (x - v0.x + y - v0.y) + c0.g);
		uint8_t b = float(dc[2] * (x - v0.x + y - v0.y) + c0.b);
		if (x >= 0 && x < img.width() && y >= 0 && y < img.height())
			img.set(x, y, { r, g, b });
		if (D > 0)
		{
			x = x + xi;
			D = D + (2 * (dx - dy));
		}
		else
			D = D + 2 * dx;
	}
}

void draw_lines(vector<vec2> m_Vertices, Image& img)
{
	for (unsigned int i = 0; i < m_Vertices.size() - 1; i += 1)
	{
		vec2 v0 = m_Vertices[i];
		vec2 v1 = m_Vertices[i + 1];

		if (abs(v1.y - v0.y) < abs(v1.x - v0.x))
		{
			if (v0.x > v1.x)
				draw_line_low(v1, v0, img);
			else
				draw_line_low(v0, v1, img);
		}
		else
		{
			if (v0.y > v1.y)
				draw_line_high(v1, v0, img);
			else
				draw_line_high(v0, v1, img);
		}
	}
}