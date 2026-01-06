// 汉字字模生成插件主入口文件
const fs = require('fs');
const path = require('path');
const { createCanvas } = require('canvas');

/**
 * 汉字字模生成插件
 * 自动检索工程中文件中的OLED_Printf函数中的中文，生成对应的字模数据
 */

// 插件实现
class ChineseFontMatrixPlugin {
  // 插件API实例
  api = null;
  
  // 配置对象
  config = null;

  // 插件初始化
  init(api) {
    console.log('汉字字模生成插件初始化');
    this.api = api;
    
    // 从API获取配置
    this.config = {
      font_name: api.getConfig('font_name') || 'SimHei',
      font_size: api.getConfig('font_size') || 16,
      mode: api.getConfig('mode') || '行列式',
      code_type: api.getConfig('code_type') || '阴码',
      bit_order: api.getConfig('bit_order') || '低位在前',
      last_project_path: api.getConfig('last_project_path') || '.',
      clear_existing_fonts: api.getConfig('clear_existing_fonts') || false,
      duplicate_handling: api.getConfig('duplicate_handling') || 'ask',
      remember_choice: api.getConfig('remember_choice') || false
    };
    
    // 注册分页数据源 - 示例：用于展示已生成的字模数据
    if (api.registerPaginatedSource) {
      api.registerPaginatedSource('generated-chars', {
        getData: async (page, pageSize, filters) => {
          console.log(`获取分页数据: 页码 ${page}, 每页大小 ${pageSize}, 筛选条件:`, filters);
          
          // 示例数据：实际应用中应从存储或数据库获取
          const total = 100;
          const data = [];
          
          // 根据page和pageSize计算数据范围
          const start = (page - 1) * pageSize;
          const end = start + pageSize;
          
          // 填充示例数据
          for (let i = start; i < end && i < total; i++) {
            // 生成示例汉字
            const char = String.fromCharCode(0x4e00 + i); // 从'一'开始
            data.push({
              id: i + 1,
              char: char,
              fontName: this.config.font_name,
              fontSize: this.config.font_size,
              generatedAt: new Date().toISOString()
            });
          }
          
          return {
            total,
            data
          };
        }
      });
    }
    
    // 显示初始化通知
    this.api.showNotification('汉字字模生成插件已初始化');
  }

  // 插件激活（独立插件不会调用此方法）
  activate() {
    console.log('汉字字模生成插件激活');
  }

  // 插件停用（独立插件不会调用此方法）
  deactivate() {
    console.log('汉字字模生成插件停用');
    
    // 清理资源
    this.api = null;
    this.config = null;
  }

  // 保存配置
  saveConfig() {
    for (const [key, value] of Object.entries(this.config)) {
      this.api.setConfig(key, value);
    }
  }

  // 搜索指定文件夹下所有文件中的OLED_Printf函数中的中文
  searchChineseInFiles(searchDirs) {
    const chineseChars = new Set();

    for (const searchDir of searchDirs) {
      this._traverseDirectory(searchDir, (filePath) => {
        if (filePath.endsWith('.c') || filePath.endsWith('.h')) {
          try {
            const content = fs.readFileSync(filePath, 'utf-8');
            this._extractChineseFromContent(content, chineseChars);
          } catch (error) {
            // 尝试使用gbk编码
            try {
              const content = fs.readFileSync(filePath, 'gbk');
              this._extractChineseFromContent(content, chineseChars);
            } catch (e) {
              console.error(`读取文件 ${filePath} 失败: ${e}`);
            }
          }
        }
      });
    }

    return Array.from(chineseChars).sort();
  }

  // 遍历目录
  _traverseDirectory(dir, callback) {
    if (!fs.existsSync(dir)) {
      console.error(`目录不存在: ${dir}`);
      return;
    }

    try {
      const files = fs.readdirSync(dir);
      // 确保files是数组
      if (Array.isArray(files)) {
        for (const file of files) {
          try {
            const filePath = path.join(dir, file);
            const stat = fs.statSync(filePath);
            if (stat.isDirectory()) {
              this._traverseDirectory(filePath, callback);
            } else {
              callback(filePath);
            }
          } catch (statError) {
            console.error(`处理文件 ${file} 时出错: ${statError.message}`);
          }
        }
      } else {
        console.error(`读取目录 ${dir} 返回的不是数组: ${typeof files}`);
      }
    } catch (readdirError) {
      console.error(`读取目录 ${dir} 失败: ${readdirError.message}`);
    }
  }

  // 从内容中提取中文
  _extractChineseFromContent(content, chineseChars) {
    // 找到所有OLED_Printf调用
    const printfMatches = content.matchAll(/OLED_Printf\([^;]*?\);/g);
    for (const match of printfMatches) {
      const printfCall = match[0];
      // 提取第一个字符串参数中的中文
      const strMatch = printfCall.match(/"([^"]*)"/);
      if (strMatch) {
        const firstStr = strMatch[1];
        // 提取字符串中的中文和特殊字符
        const specialChars = firstStr.match(/[\u4e00-\u9fa5℃°℉]+/g) || [];
        for (const chars of specialChars) {
          for (const char of chars) {
            chineseChars.add(char);
          }
        }
      }
      
      // 查找函数参数中可能的条件表达式里的中文
      const conditionMatches = printfCall.matchAll(/\?"([^"?]*)"\s*:\s*"([^"]*)"/g);
      for (const [, truePart, falsePart] of conditionMatches) {
        // 提取条件表达式中的中文和特殊字符
        const specialChars = (truePart + falsePart).match(/[\u4e00-\u9fa5℃°℉]+/g) || [];
        for (const chars of specialChars) {
          for (const char of chars) {
            chineseChars.add(char);
          }
        }
      }
    }
  }

  // 生成单个汉字的点阵数据
  generateCharBitmap(char) {
    console.log(`生成汉字 '${char}' 的字模数据`);
    
    const size = this.config.font_size;
    const bitmapData = [];
    
    // 创建canvas，大小为字体大小×字体大小
    const canvas = createCanvas(size, size);
    const ctx = canvas.getContext('2d');
    
    // 设置背景为白色
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, size, size);
    
    // 设置字体样式
    ctx.fillStyle = '#000000';
    ctx.font = `${size}px ${this.config.font_name}`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    
    // 绘制汉字
    ctx.fillText(char, size / 2, size / 2);
    
    // 获取像素数据
    const imageData = ctx.getImageData(0, 0, size, size);
    const data = imageData.data;
    
    if (this.config.mode === "列行式") {
      // 列行式生成：按列扫描，每列按行排列
      for (let col = 0; col < size; col++) {
        // 每列分两行，每行8个像素
        for (let rowBlock = 0; rowBlock < size; rowBlock += 8) {
          let byte = 0;
          for (let bit = 0; bit < 8; bit++) {
            const row = rowBlock + bit;
            if (row < size) {
              // 获取像素值，0表示黑色，255表示白色
              const pixelIndex = (row * size + col) * 4;
              const pixel = data[pixelIndex]; // 取红色通道值
              
              // 判断像素是否为黑色（阈值设为128）
              let isBlack = pixel < 128;
              
              // 处理阴码/阳码
              if (this.config.code_type === "阴码") {
                isBlack = !isBlack;
              }
              
              // 设置对应位
              if (isBlack) {
                if (this.config.bit_order === "低位在前") {
                  byte |= (1 << bit);
                } else {
                  byte |= (1 << (7 - bit));
                }
              }
            }
          }
          bitmapData.push(byte);
        }
      }
    } else { // 行列式
      // 行列式生成：按行扫描，每行按列排列
      for (let row = 0; row < size; row += 8) {
        // 每行分8个像素
        for (let col = 0; col < size; col++) {
          let byte = 0;
          for (let bit = 0; bit < 8; bit++) {
            const currentRow = row + bit;
            if (currentRow < size) {
              // 获取像素值
              const pixelIndex = (currentRow * size + col) * 4;
              const pixel = data[pixelIndex];
              
              // 判断像素是否为黑色
              let isBlack = pixel < 128;
              
              // 处理阴码/阳码
              if (this.config.code_type === "阴码") {
                isBlack = !isBlack;
              }
              
              // 设置对应位
              if (isBlack) {
                if (this.config.bit_order === "低位在前") {
                  byte |= (1 << bit);
                } else {
                  byte |= (1 << (7 - bit));
                }
              }
            }
          }
          bitmapData.push(byte);
        }
      }
    }
    
    return bitmapData;
  }

  // 更新OLED_Data.c文件中的中文字库
  updateOledDataFile(chars, projectDir, clearExisting = false, duplicateHandling = "ask") {
    try {
      const oledDataFile = path.join(projectDir, 'HardWare', 'OLED_Data.c');
      console.log(`尝试读取文件: ${oledDataFile}`);
      
      // 检查文件是否存在
      if (!fs.existsSync(oledDataFile)) {
        throw new Error(`文件不存在: ${oledDataFile}`);
      }
        
      const content = fs.readFileSync(oledDataFile, 'utf-8');
      console.log(`成功读取文件，长度: ${content.length} 字符`);

      // 查找并替换中文字库部分
      const startPattern = /const ChineseCell_t OLED_CF16x16\[\] = \{/;
      const endPattern = /\};/;

      let startMatch = content.match(startPattern);
      let endMatch;

      if (startMatch) {
        endMatch = content.slice(startMatch.index + startMatch[0].length).match(endPattern);
      } else {
        // 尝试其他可能的格式
        const alternativePatterns = [
          /const\s+uint8_t\s+OLED_CF16x16\[\]\s*=\s*\{/,
          /uint8_t\s+OLED_CF16x16\[\]\s*=\s*\{/,
          /static\s+const\s+uint8_t\s+OLED_CF16x16\[\]\s*=\s*\{/
        ];
        
        for (const pattern of alternativePatterns) {
          startMatch = content.match(pattern);
          if (startMatch) {
            endMatch = content.slice(startMatch.index + startMatch[0].length).match(endPattern);
            if (endMatch) {
              break;
            }
          }
        }
      }

      if (startMatch && endMatch) {
        const startPos = startMatch.index + startMatch[0].length;
        const endPos = startPos + endMatch.index + endMatch[0].length;

        // 如果不需要清空现有字模，则先读取现有字模
        const existingFonts = {};
        if (!clearExisting) {
          // 提取现有字模
          const fontPattern = /\{"([^"]+)", \{([^}]*)\}, 16, 16\},?/g;
          let match;
          while ((match = fontPattern.exec(content.slice(startPos, endPos))) !== null) {
            if (match[1] && match[1] !== "") { // 排除默认图形和结束标志
              existingFonts[match[1]] = match[2];
            }
          }
        }

        // 生成新的字模数据
        const fontData = [];
        
        for (const char of chars) {
          // 检查是否重复
          if (char in existingFonts) {
            // 这里简化处理，直接覆盖
            const bitmapData = this.generateCharBitmap(char);
            const dataStr = bitmapData.map(byte => `0x${byte.toString(16).padStart(2, '0')}`).join(',');
            fontData.push(`    {"${char}", {${dataStr}}, 16, 16},`);
            delete existingFonts[char];
          } else {
            // 新的字模，直接添加
            const bitmapData = this.generateCharBitmap(char);
            const dataStr = bitmapData.map(byte => `0x${byte.toString(16).padStart(2, '0')}`).join(',');
            fontData.push(`    {"${char}", {${dataStr}}, 16, 16},`);
          }
        }

        // 如果不需要清空现有字模，添加剩余的现有字模
        if (!clearExisting) {
          for (const [char, data] of Object.entries(existingFonts)) {
            fontData.push(`    {"${char}", { ${data} }, 16, 16},`);
          }
        }

        // 添加默认图形和结束标志
        fontData.push("    {\"\\0\", {0xFF,0x01,0x01,0x01,0x31,0x09,0x09,0x09,0x09,0x89,0x71,0x01,0x01,0x01,0x01,0xFF, 0xFF,0x80,0x80,0x80,0x80,0x80,0x80,0x96,0x81,0x80,0x80,0x80,0x80,0x80,0x80,0xFF}, 16, 16},");
        fontData.push("    {NULL, {0}, 0, 0} // 结束标志");
        fontData.push("};");

        const newFontContent = '\n' + fontData.join('\n') + '\n';
        const newContent = content.slice(0, startPos) + newFontContent + content.slice(endPos);

        // 写入更新后的内容
        fs.writeFileSync(oledDataFile, newContent, 'utf-8');

        console.log(`成功更新 ${oledDataFile} 文件中的中文字库`);
        console.log(`共生成 ${chars.length} 个汉字的字模数据`);
        return true;
      } else {
        console.log("未找到中文字库定义，无法更新");
        return false;
      }

    } catch (error) {
      console.error(`更新文件失败: ${error}`);
      return false;
    }
  }

  // 设置配置
  setConfig(key, value) {
    this.config[key] = value;
    this.api.setConfig(key, value);
  }

  // 获取配置
  getConfig(key) {
    return this.config[key];
  }
  
  // 更新OLED_Data.c文件中的图模数据
  updateOledImageDataFile(params) {
    try {
      // 解析参数
      const { projectPath, imageData, arrayName, width, height, overwrite } = params;
      const projectDir = projectPath;
      
      console.log('开始更新OLED_Data.c文件中的图模数据', {
        projectDir,
        arrayName,
        width,
        height,
        imageDataLength: imageData.length
      });
      
      // 检查参数
      if (!projectDir) {
        throw new Error('未提供工程路径');
      }
      
      if (!imageData || !Array.isArray(imageData)) {
        throw new Error('图模数据格式错误');
      }
      
      // 尝试多种可能的文件路径
      const possiblePaths = [
        path.join(projectDir, 'HardWare', 'OLED_Data.c'),
        path.join(projectDir, 'OLED_Data.c'),
        path.join(projectDir, 'hardware', 'OLED_Data.c')
      ];
      
      let oledDataFile = null;
      for (const possiblePath of possiblePaths) {
        if (fs.existsSync(possiblePath)) {
          oledDataFile = possiblePath;
          break;
        }
      }
      
      if (!oledDataFile) {
        console.error(`未找到OLED_Data.c文件，尝试了以下路径: ${possiblePaths.join(', ')}`);
        return false;
      }
      
      console.log(`找到OLED_Data.c文件: ${oledDataFile}`);
      
      const content = fs.readFileSync(oledDataFile, 'utf-8');
      console.log(`成功读取文件，长度: ${content.length} 字符`);

      // 检查是否已存在同名数组
      // 使用更严格的正则表达式，确保能匹配到所有可能的数组定义格式
      // 注意：在字符串模板中，\s 必须写成 \\s，否则会被错误解析
      const arrayPattern = new RegExp(`const\\s+uint8_t\\s+${arrayName}\\s*\\[\\]\\s*=\\s*\\{`, 'g');
      const matches = content.match(arrayPattern);
      const arrayExists = matches && matches.length > 0;
      
      // 如果数组已存在且没有覆盖标志，返回提示信息
      if (arrayExists && overwrite !== true) {
        console.log(`数组 ${arrayName} 已存在于文件中，共找到 ${matches.length} 个匹配`);
        return {
          success: false,
          error: 'ArrayAlreadyExists',
          message: `数组 ${arrayName} 已存在，请选择是否覆盖`,
          arrayName: arrayName,
          matchCount: matches.length
        };
      }
      
      // 如果需要覆盖，先移除所有原有数组
      let processedContent = content;
      if (arrayExists && overwrite === true) {
        console.log(`开始移除已存在的数组 ${arrayName}，共找到 ${matches.length} 个匹配`);
        
        // 匹配整个数组定义（包括大括号和内容）
        // 使用更可靠的正则表达式，确保能正确匹配整个数组定义
        let tempContent = content;
        let modified = false;
        let removeCount = 0;
        
        // 先找到所有匹配，然后逆序移除，避免索引偏移
        const allMatches = [];
        const fullArrayPattern = new RegExp(`const\\s+uint8_t\\s+${arrayName}\\s*\\[\\]\\s*=\\s*\\{([\\s\\S]*?)\\};`, 'g');
        let fullMatch;
        while ((fullMatch = fullArrayPattern.exec(tempContent)) !== null) {
          allMatches.push(fullMatch);
        }
        
        console.log(`完整匹配到 ${allMatches.length} 个数组定义`);
        
        // 逆序移除所有匹配项
        for (let i = allMatches.length - 1; i >= 0; i--) {
          const match = allMatches[i];
          const before = tempContent.slice(0, match.index);
          const after = tempContent.slice(match.index + match[0].length);
          tempContent = before + after;
          modified = true;
          removeCount++;
        }
        
        if (modified) {
          processedContent = tempContent;
          console.log(`已成功移除 ${removeCount} 个数组 ${arrayName} 的实例`);
        } else {
          console.log(`未找到需要移除的数组实例`);
        }
      }
      
      // 查找图像数据插入位置
      const imageDataPatterns = [
        /\/\*按照上面的格式，在这个位置加入新的图像数据\*\//,
        /\/\*图像数据插入位置\*\//,
        /\/\*在这里添加新的图像数据\*\//,
        /\/\*新图像数据位置\*\//
      ];
      
      let match = null;
      for (const pattern of imageDataPatterns) {
        match = processedContent.match(pattern);
        if (match) {
          console.log(`找到图像数据插入位置，使用模式: ${pattern.toString()}`);
          break;
        }
      }
      
      // 如果没有找到特定注释，尝试在文件末尾添加
      if (!match) {
        console.log('未找到特定注释，尝试在文件末尾添加图模数据');
        match = { index: processedContent.length, 0: '' };
      }
      
      const insertPos = match.index + match[0].length;
      
      // 生成图模数据字符串
      const imageDataLines = [];
      imageDataLines.push(`\n\nconst uint8_t ${arrayName}[] = {`);
      
      // 格式化数据，每行8个字节
      let line = '\t';
      for (let i = 0; i < imageData.length; i++) {
        const byte = imageData[i];
        const hexStr = `0X${byte.toString(16).toUpperCase().padStart(2, '0')}`;
        line += hexStr;
        
        if (i < imageData.length - 1) {
          line += ',';
          if ((i + 1) % 8 === 0) {
            imageDataLines.push(line);
            line = '\t';
          } else {
            line += ' ';
          }
        }
      }
      
      // 添加最后一行数据
      if (line.trim()) {
        imageDataLines.push(line);
      }
      
      imageDataLines.push('};');
      
      const newImageContent = imageDataLines.join('\n');
      const newContent = processedContent.slice(0, insertPos) + newImageContent + processedContent.slice(insertPos);

      // 写入更新后的内容
      fs.writeFileSync(oledDataFile, newContent, 'utf-8');
      console.log(`成功更新 ${oledDataFile} 文件中的图模数据`);
      console.log(`生成了 ${imageData.length} 字节的图模数据，数组名称: ${arrayName}`);
      
      // 处理OLED_Data.h文件
      console.log('开始更新OLED_Data.h文件中的extern声明');
      
      // 构建OLED_Data.h文件的可能路径
      const hPossiblePaths = [
        path.join(projectDir, 'HardWare', 'OLED_Data.h'),
        path.join(projectDir, 'OLED_Data.h'),
        path.join(projectDir, 'hardware', 'OLED_Data.h')
      ];
      
      let oledHeaderFile = null;
      for (const possiblePath of hPossiblePaths) {
        if (fs.existsSync(possiblePath)) {
          oledHeaderFile = possiblePath;
          break;
        }
      }
      
      if (!oledHeaderFile) {
        console.error(`未找到OLED_Data.h文件，尝试了以下路径: ${hPossiblePaths.join(', ')}`);
        return {
          success: true,
          filePath: oledDataFile,
          overwrite: overwrite,
          warning: '未找到OLED_Data.h文件，无法添加extern声明'
        };
      }
      
      console.log(`找到OLED_Data.h文件: ${oledHeaderFile}`);
      
      // 读取OLED_Data.h文件内容
      const hContent = fs.readFileSync(oledHeaderFile, 'utf-8');
      
      // 检查是否已存在同名extern声明
      const externPattern = new RegExp(`extern\s+const\s+uint8_t\s+${arrayName}\s*\[\]\s*;`, 'g');
      const externMatches = hContent.match(externPattern);
      const externExists = externMatches && externMatches.length > 0;
      
      let processedHContent = hContent;
      
      // 如果需要覆盖或已存在，先移除原有声明
      if (externExists) {
        console.log(`已存在${externMatches.length}个${arrayName}的extern声明，准备移除`);
        processedHContent = processedHContent.replace(externPattern, '');
      }
      
      // 查找// 图像数据注释位置（注意：没有逗号）
      const imageDataCommentPattern = /\/\/ 图像数据/;
      const commentMatch = processedHContent.match(imageDataCommentPattern);
      
      if (!commentMatch) {
        console.error('未找到// 图像数据注释');
        return {
          success: true,
          filePath: oledDataFile,
          overwrite: overwrite,
          warning: '未找到// 图像数据注释，无法添加extern声明'
        };
      }
      
      const commentPos = commentMatch.index + commentMatch[0].length;
      
      // 生成extern声明
      const externDecl = `\nextern const uint8_t ${arrayName}[];`;
      
      // 在注释后面添加extern声明
      const newHContent = processedHContent.slice(0, commentPos) + externDecl + processedHContent.slice(commentPos);
      
      // 写入更新后的OLED_Data.h文件
      fs.writeFileSync(oledHeaderFile, newHContent, 'utf-8');
      console.log(`成功更新 ${oledHeaderFile} 文件，添加了${arrayName}的extern声明`);
      
      return {
        success: true,
        filePath: oledDataFile,
        headerFilePath: oledHeaderFile,
        overwrite: overwrite,
        arrayName: arrayName
      };
    } catch (error) {
      console.error(`更新文件失败: ${error}`);
      console.error(`错误详情: ${error.stack}`);
      return {
        success: false,
        error: error.message
      };
    }
  }

  // 生成字模
  async generate(params) {
    console.log('开始生成字模:', params);
    
    // 更新配置
    this.config.font_name = params.fontName;
    this.config.font_size = params.fontSize;
    this.config.mode = params.mode;
    this.config.code_type = params.codeType;
    this.config.bit_order = params.bitOrder;
    this.config.last_project_path = params.projectPath;
    this.config.clear_existing_fonts = params.clearExisting;
    this.config.duplicate_handling = params.duplicateHandling;
    this.config.remember_choice = params.rememberChoice;
    
    // 保存配置
    for (const [key, value] of Object.entries(this.config)) {
      this.api.setConfig(key, value);
    }
    
    let chineseChars = [];
    
    // 根据生成方式搜索中文
    if (params.generateMode === 'search' || params.generateMode === 'both') {
      console.log('搜索代码中的汉字...');
      
      // 构建完整的搜索目录路径
      const searchDirs = [
        path.join(params.projectPath, 'User'),
        path.join(params.projectPath, 'HardWare')
      ];
      
      // 搜索代码中的中文
      const codeChars = this.searchChineseInFiles(searchDirs);
      chineseChars = chineseChars.concat(codeChars);
    }
    
    // 处理手动输入的汉字
    if (params.generateMode === 'manual' || params.generateMode === 'both') {
      console.log('处理手动输入的汉字...');
      if (params.manualChars) {
        // 提取所有汉字
        const manualChineseChars = params.manualChars.match(/[\u4e00-\u9fa5℃°℉]+/g) || [];
        for (const chars of manualChineseChars) {
          for (const char of chars) {
            chineseChars.push(char);
          }
        }
      }
    }
    
    // 去重并排序
    chineseChars = [...new Set(chineseChars)].sort();
    
    if (chineseChars.length === 0) {
      throw new Error('未找到任何中文汉字');
    }
    
    console.log(`找到 ${chineseChars.length} 个中文汉字: ${chineseChars.join('')}`);
    
    // 更新OLED_Data.c文件
    const success = this.updateOledDataFile(
      chineseChars, 
      params.projectPath, 
      params.clearExisting, 
      params.duplicateHandling
    );
    
    if (!success) {
      throw new Error('更新OLED_Data.c文件失败');
    }
    
    console.log('字模生成成功');
    
    // 生成每个汉字的点阵数据用于前端预览
    const charMatrixData = {};
    for (const char of chineseChars) {
      charMatrixData[char] = this.generateCharBitmap(char);
    }
    
    return {
      foundChars: chineseChars,
      charMatrixData: charMatrixData,
      success: true
    };
  }
}

// 导出插件实例
const pluginInstance = new ChineseFontMatrixPlugin();

module.exports = {
  init: (api) => pluginInstance.init(api),
  activate: () => pluginInstance.activate(),
  deactivate: () => pluginInstance.deactivate(),
  generate: (params) => pluginInstance.generate(params),
  updateOledImageDataFile: (params) => pluginInstance.updateOledImageDataFile(params)
};