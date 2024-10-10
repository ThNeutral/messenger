import React, { useState } from 'react'


export const LoupeIcon = ({ color = "#696a6a", hoveredColor = '#EBEFEC' }) => {
    const [isHovered, setIsHovered] = useState(false);
    const onMouseEnter = () => setIsHovered(true);
    const onMouseLeave = () => setIsHovered(false);
    return (
      <svg onMouseEnter={onMouseEnter} onMouseLeave={onMouseLeave}
      width="800px"
      height="800px"
      viewBox="0 0 8.4666669 8.4666669"
      id="svg8"
      xmlns="http://www.w3.org/2000/svg"
    >
      <defs id="defs2" />
      <g id="layer1" transform="translate(0,-288.53332)">
        <path
          d="M 11.996094 1.0039062 C 5.9328116 1.0039062 0.99610131 5.9386839 0.99609375 12.001953 C 0.99610131 18.06526 5.9328116 23.001953 11.996094 23.001953 C 14.670102 23.001953 17.122499 22.040573 19.03125 20.447266 L 29.291016 30.708984 C 30.235897 31.653866 31.653866 30.235898 30.708984 29.291016 L 20.447266 19.029297 C 22.03584 17.121901 22.994137 14.671545 22.994141 12.001953 C 22.994133 5.9386839 18.059376 1.0039062 11.996094 1.0039062 z M 11.996094 3.0039062 C 16.978497 3.003944 20.994135 7.0195531 20.994141 12.001953 C 20.994135 16.984391 16.978497 21.001953 11.996094 21.001953 C 7.0136911 21.001953 2.9960999 16.984391 2.9960938 12.001953 C 2.9960999 7.0195531 7.0136911 3.003944 11.996094 3.0039062 z "
          id="path935"
          style={{
            color: "#000000",
            fontStyle: "normal",
            fontVariant: "normal",
            fontWeight: "normal",
            fontStretch: "normal",
            fontSize: "medium",
            lineHeight: "normal",
            fontFamily: "sans-serif",
            fontVariantLigatures: "normal",
            fontVariantPosition: "normal",
            fontVariantCaps: "normal",
            fontVariantNumeric: "normal",
            fontVariantAlternates: "normal",
            fontFeatureSettings: "normal",
            textIndent: 0,
            textAlign: "start",
            textDecoration: "none",
            textDecorationLine: "none",
            textDecorationStyle: "solid",
            textDecorationColor: "#000000",
            letterSpacing: "normal",
            wordSpacing: "normal",
            textTransform: "none",
            direction: "ltr",
            textOrientation: "mixed",
            dominantBaseline: "auto",
            baselineShift: "baseline",
            textAnchor: "start",
            whiteSpace: "normal",
            clipRule: "nonzero",
            display: "inline",
            overflow: "visible",
            visibility: "visible",
            opacity: 1,
            isolation: "auto",
            mixBlendMode: "normal",
            colorInterpolation: "sRGB",
            vectorEffect: "none",
            fill: isHovered ? hoveredColor : color,
            fillOpacity: 1,
            fillRule: "nonzero",
            stroke : isHovered ? hoveredColor : color,
            strokeWidth: 1,
            strokeLinecap: "round",
            strokeLinejoin: "round",
            strokeMiterlimit: 4,
            strokeDasharray: "none",
            strokeDashoffset: 0,
            strokeOpacity: 1,
            paintOrder: "stroke fill markers",
            colorRendering: "auto",
            imageRendering: "auto",
            shapeRendering: "auto",
            textRendering: "auto",
          }}
          transform="matrix(0.26458333,0,0,0.26458333,0,288.53332)"
        />
      </g>
    </svg>
      )
}
