#ifndef image_H_
#define image_H_

#include "glm/glm.hpp"
#include <vector>
#include <iostream>

// This is a placeholder class
// Feel free to replace this clas with your own Image class

struct Pixel
{
    unsigned char r = 0;
    unsigned char g = 0;
    unsigned char b = 0;
    static Pixel black;
    static Pixel white;
};

class Image
{
public:
      
    Image();
    Image(unsigned int width, unsigned int height);
    //Image(const Image& orig);
    Image& operator=(const Image& orig);

    virtual ~Image();

    // save the given filename
    bool save(const std::string& filename) const;

    // return the current width
    inline int width() const { return m_Width; }

    // return the current height
    inline int height() const { return m_Height; }

    // Get the pixel at index (row, col)
    Pixel get(int row, int col) const;

    // Get the pixel at index (row, col)
    void set(int row, int col, const Pixel& color); 

private:
    Pixel* m_Data;
    unsigned int m_Width;
    unsigned int m_Height;
};

void draw_lines(std::vector<glm::vec2> m_Vertices, Image& img);

#endif