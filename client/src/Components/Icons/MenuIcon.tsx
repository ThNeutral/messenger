import React, { useState } from 'react'


export const MenuIcon = ({ color = "#696a6a", hoveredColor = '#D72D77' }) => {
    const [isHovered, setIsHovered] = useState(false);
    const onMouseEnter = () => setIsHovered(true);
    const onMouseLeave = () => setIsHovered(false);
    return (
    <svg width="800px" height="800px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" onMouseEnter={onMouseEnter} onMouseLeave={onMouseLeave}>
        <path 
          d="M4 6H20M4 12H20M4 18H20" 
          stroke={isHovered ? hoveredColor : color}
          strokeWidth="2" 
          strokeLinecap="round" 
          strokeLinejoin="round"
        />
      </svg>
      )
}
