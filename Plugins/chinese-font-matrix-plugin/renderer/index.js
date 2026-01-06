// 汉字字模生成插件UI组件
// 纯JavaScript实现，不使用JSX

/**
 * 汉字字模插件函数 - 直接返回DOM元素，符合工具箱要求
 */
const ChineseFontMatrixPlugin = function() {
  // 状态管理
  let state = {
    projectPath: '.',
    fontName: '黑体',
    fontSize: 16,
    mode: '行列式',
    codeType: '阴码',
    bitOrder: '低位在前',
    manualChars: '',
    generateMode: 'search',
    clearExisting: false,
    duplicateHandling: 'ask',
    rememberChoice: false,
    status: '就绪',
    foundChars: [],
    isGenerating: false,
    // 当前激活的页面
    activePage: 'main', // main - 主页面, test - 测试页面
    // 分页状态 - 用于找到的汉字
    currentPage: 1,
    pageSize: 20,
    totalPages: 1,
    // 测试分页状态
    testCurrentPage: 1,
    testPageSize: 10,
    testTotalPages: 1,
    testData: [],
    isLoadingTestData: false,
    // 生成的图像数据
    generatedImageData: null,
    // 字模预览数据
    selectedChar: '',
    charMatrixData: {} // 存储每个汉字的点阵数据
  };

  // 字体映射表（中文 -> 英文/拼音）
  const FONT_NAME_MAP = {
    "黑体": "SimHei",
    "微软雅黑": "Microsoft YaHei",
    "宋体": "SimSun",
    "仿宋": "FangSong",
    "楷体": "KaiTi",
    "华文宋体": "STSong",
    "华文中宋": "STZhongsong",
    "华文楷体": "STKaiti",
    "华文细黑": "STXihei",
    "华文仿宋": "STFangsong",
    "华文彩云": "STCaiyun",
    "华文行楷": "STXingkai",
    "Arial Unicode MS": "Arial Unicode MS",
    "幼圆": "SimYou",
    "新宋体": "NSimSun"
  };

  // 插件API和ID
  let pluginApi = window.pluginApi;
  let pluginId = 'chinese-font-matrix-plugin';
  
  // 创建主容器
  const container = document.createElement('div');
  container.className = 'chinese-font-matrix-plugin';
  
  // 立即注册插件页面，在渲染函数返回之前完成
  if (pluginApi) {
    console.log('开始注册插件页面');
    // 依次注册所有页面（使用顺序注册，确保每个页面都注册成功）
    pluginApi.registerPage(pluginId, 'main', {
      title: '字模生成',
      icon: '',
      order: 1
    }).then(() => {
      console.log('注册字模生成页面成功');
      return pluginApi.registerPage(pluginId, 'test', {
        title: '图模生成',
        icon: '',
        order: 2
      });
    }).then(() => {
      console.log('注册图模生成页面成功');
      return pluginApi.registerPage(pluginId, 'subpage1', {
        title: '设置',
        icon: '',
        order: 3
      });
    }).then(() => {
      console.log('注册设置页面成功');
      return pluginApi.registerPage(pluginId, 'subpage2', {
        title: '子页面2',
        icon: '',
        order: 4
      });
    }).then(() => {
      console.log('注册子页面2成功');
      return pluginApi.registerPage(pluginId, 'subpage3', {
        title: '子页面3',
        icon: '',
        order: 5
      });
    }).then(() => {
      console.log('注册子页面3成功');
      console.log('所有插件页面注册完成');
    }).catch(error => {
      console.error('注册插件页面失败:', error);
    });
  }
  
  // 初始化配置
  const loadConfig = function(callback) {
    try {
      // 检查pluginApi是否可用
      if (!pluginApi) {
        console.warn('插件API不可用，使用默认配置');
        updateStatus('使用默认配置');
        // 使用默认配置
        state.fontName = state.fontName || '黑体';
        state.fontSize = state.fontSize || 16;
        state.mode = state.mode || '行列式';
        state.codeType = state.codeType || '阴码';
        state.bitOrder = state.bitOrder || '低位在前';
        state.projectPath = state.projectPath || '.';
        state.clearExisting = state.clearExisting || false;
        state.duplicateHandling = state.duplicateHandling || 'ask';
        state.rememberChoice = state.rememberChoice || false;
        if (callback) callback();
        return;
      }
      
      // 从插件API获取配置，使用Promise链
      pluginApi.getConfig(pluginId, 'font_name').then(function(fontName) {
        state.fontName = fontName || '黑体';
        return pluginApi.getConfig(pluginId, 'font_size');
      }).then(function(fontSize) {
        state.fontSize = parseInt(fontSize || '16');
        return pluginApi.getConfig(pluginId, 'mode');
      }).then(function(mode) {
        state.mode = mode || '行列式';
        return pluginApi.getConfig(pluginId, 'code_type');
      }).then(function(codeType) {
        state.codeType = codeType || '阴码';
        return pluginApi.getConfig(pluginId, 'bit_order');
      }).then(function(bitOrder) {
        state.bitOrder = bitOrder || '低位在前';
        return pluginApi.getConfig(pluginId, 'last_project_path');
      }).then(function(projectPath) {
        state.projectPath = projectPath || '.';
        return pluginApi.getConfig(pluginId, 'clear_existing_fonts');
      }).then(function(clearExisting) {
        state.clearExisting = clearExisting || false;
        return pluginApi.getConfig(pluginId, 'duplicate_handling');
      }).then(function(duplicateHandling) {
        state.duplicateHandling = duplicateHandling || 'ask';
        return pluginApi.getConfig(pluginId, 'remember_choice');
      }).then(function(rememberChoice) {
        state.rememberChoice = rememberChoice || false;
        updateStatus('就绪');
        if (callback) callback();
      }).catch(function(error) {
        console.error('加载配置失败:', error);
        updateStatus('加载配置失败');
        // 使用默认配置继续运行
        state.fontName = state.fontName || '黑体';
        state.fontSize = state.fontSize || 16;
        state.mode = state.mode || '行列式';
        state.codeType = state.codeType || '阴码';
        state.bitOrder = state.bitOrder || '低位在前';
        state.projectPath = state.projectPath || '.';
        state.clearExisting = state.clearExisting || false;
        state.duplicateHandling = state.duplicateHandling || 'ask';
        state.rememberChoice = state.rememberChoice || false;
        if (callback) callback();
      });
    } catch (error) {
      console.error('加载配置失败:', error);
      updateStatus('加载配置失败');
      // 使用默认配置继续运行
      state.fontName = state.fontName || '黑体';
      state.fontSize = state.fontSize || 16;
      state.mode = state.mode || '行列式';
      state.codeType = state.codeType || '阴码';
      state.bitOrder = state.bitOrder || '低位在前';
      state.projectPath = state.projectPath || '.';
      state.clearExisting = state.clearExisting || false;
      state.duplicateHandling = state.duplicateHandling || 'ask';
      state.rememberChoice = state.rememberChoice || false;
      if (callback) callback();
    }
  };

  // 处理工程目录浏览
  const handleBrowseProject = function() {
    try {
      const electronAPI = window.electronAPI;
      if (!electronAPI) {
        throw new Error('Electron API不可用');
      }
      
      electronAPI.browseDirectory({
        title: '选择工程目录',
        defaultPath: state.projectPath,
        properties: ['openDirectory']
      }).then(function(result) {
        if (result && result.canceled === false && result.filePaths.length > 0) {
          state.projectPath = result.filePaths[0];
          renderProjectPath();
        }
      }).catch(function(error) {
        console.error('浏览目录失败:', error);
        alert('浏览目录失败: ' + error.message);
      });
    } catch (error) {
      console.error('浏览目录失败:', error);
      alert('浏览目录失败: ' + error.message);
    }
  };

  // 处理生成字模
  const handleGenerate = function() {
    console.log('>>> 开始调用handleGenerate函数');
    state.isGenerating = true;
    updateStatus('正在生成字模...');
    updateGenerateButton();

    try {
      console.log('>>> 进入handleGenerate try块');
      // 获取插件API
      const electronAPI = window.electronAPI;
      console.log('>>> 检查pluginApi和electronAPI:', !!pluginApi, !!electronAPI);
      if (!pluginApi || !electronAPI) {
        throw new Error('插件API或Electron API不可用');
      }

      // 保存当前配置
      console.log('>>> 准备调用Promise.all保存配置');
      Promise.all([
        pluginApi.setConfig(pluginId, 'font_name', state.fontName),
        pluginApi.setConfig(pluginId, 'font_size', state.fontSize),
        pluginApi.setConfig(pluginId, 'mode', state.mode),
        pluginApi.setConfig(pluginId, 'code_type', state.codeType),
        pluginApi.setConfig(pluginId, 'bit_order', state.bitOrder),
        pluginApi.setConfig(pluginId, 'last_project_path', state.projectPath),
        pluginApi.setConfig(pluginId, 'clear_existing_fonts', state.clearExisting),
        pluginApi.setConfig(pluginId, 'duplicate_handling', state.duplicateHandling),
        pluginApi.setConfig(pluginId, 'remember_choice', state.rememberChoice)
      ]).then(function() {
        console.log('>>> 配置保存成功，准备调用pluginApi.generate');
        // 调用后端生成功能
        const generateParams = {
          projectPath: state.projectPath,
          fontName: FONT_NAME_MAP[state.fontName],
          fontSize: state.fontSize,
          mode: state.mode,
          codeType: state.codeType,
          bitOrder: state.bitOrder,
          manualChars: state.manualChars,
          generateMode: state.generateMode,
          clearExisting: state.clearExisting,
          duplicateHandling: state.duplicateHandling,
          rememberChoice: state.rememberChoice
        };
        console.log('>>> 调用pluginApi.generate，参数:', generateParams);
        return pluginApi.generate(generateParams);
      }).then(function(result) {
        console.log('>>> pluginApi.generate调用成功，结果:', result);
        // 更新状态和结果
        state.foundChars = result.foundChars || [];
        // 保存字模数据到状态中
        if (result.charMatrixData) {
          console.log('>>> 收到字模数据，共', Object.keys(result.charMatrixData).length, '个汉字');
          state.charMatrixData = result.charMatrixData;
        }
        console.log('>>> 设置foundChars为:', state.foundChars);
        updateStatus('成功生成 ' + (result.foundChars ? result.foundChars.length : 0) + ' 个汉字的字模数据');
        console.log('>>> 准备调用renderFoundChars函数');
        renderFoundChars();
        console.log('>>> renderFoundChars函数调用完成');
      }).catch(function(error) {
        console.error('>>> Promise链catch块捕获到错误:', error);
        console.error('>>> 错误类型:', typeof error);
        console.error('>>> 错误对象:', JSON.stringify(error, null, 2));
        // 处理不同类型的错误对象
        let errorMessage = '未知错误';
        if (error && typeof error === 'object') {
          if (error.message) {
            errorMessage = error.message;
          } else if (error.error) {
            errorMessage = error.error;
          } else if (error.toString) {
            errorMessage = error.toString();
          }
        } else if (typeof error === 'string') {
          errorMessage = error;
        }
        console.error('>>> 最终错误信息:', errorMessage);
        updateStatus('生成失败: ' + errorMessage);
      }).finally(function() {
        console.log('>>> Promise链finally块执行');
        state.isGenerating = false;
        updateGenerateButton();
      });
      console.log('>>> Promise链已启动，函数继续执行');
    } catch (error) {
      console.error('>>> try-catch块捕获到错误:', error);
      console.error('>>> 错误堆栈:', error.stack);
      updateStatus('生成失败: ' + (error.message || error.toString()));
      state.isGenerating = false;
      updateGenerateButton();
    }
    console.log('>>> handleGenerate函数执行完成，返回');
  };

  // 处理保存配置
  const handleSaveConfig = function() {
    try {
      // 获取插件API
      if (!pluginApi) {
        throw new Error('插件API不可用');
      }

      // 保存配置到插件API
      Promise.all([
        pluginApi.setConfig(pluginId, 'font_name', state.fontName),
        pluginApi.setConfig(pluginId, 'font_size', state.fontSize),
        pluginApi.setConfig(pluginId, 'mode', state.mode),
        pluginApi.setConfig(pluginId, 'code_type', state.codeType),
        pluginApi.setConfig(pluginId, 'bit_order', state.bitOrder),
        pluginApi.setConfig(pluginId, 'last_project_path', state.projectPath),
        pluginApi.setConfig(pluginId, 'clear_existing_fonts', state.clearExisting),
        pluginApi.setConfig(pluginId, 'duplicate_handling', state.duplicateHandling),
        pluginApi.setConfig(pluginId, 'remember_choice', state.rememberChoice)
      ]).then(function() {
        updateStatus('配置已成功保存');
      }).catch(function(error) {
        console.error('保存配置失败:', error);
        updateStatus('保存配置失败: ' + error.message);
      });
    } catch (error) {
      console.error('保存配置失败:', error);
      updateStatus('保存配置失败: ' + error.message);
    }
  };

  // 填充字体选项
  const fillFontOptions = function() {
    const select = container.querySelector('#font-name');
    if (!select) return;
    
    // 清空现有选项
    select.innerHTML = '';
    
    // 填充字体选项
    var fontNames = Object.keys(FONT_NAME_MAP);
    for (var i = 0; i < fontNames.length; i++) {
      var name = fontNames[i];
      const option = document.createElement('option');
      option.value = name;
      option.textContent = name;
      if (name === state.fontName) {
        option.selected = true;
      }
      select.appendChild(option);
    }
  };
  
  // 设置初始值
  const setInitialValues = function() {
    // 设置工程路径
    const projectPathInput = container.querySelector('#project-path');
    if (projectPathInput) {
      projectPathInput.value = state.projectPath;
    }
    
    // 设置字体大小
    const fontSizeInput = container.querySelector('#font-size');
    if (fontSizeInput) {
      fontSizeInput.value = state.fontSize;
    }
    
    // 设置取模方式
    const modeSelect = container.querySelector('#mode');
    if (modeSelect) {
      modeSelect.value = state.mode;
    }
    
    // 设置码制
    const codeTypeSelect = container.querySelector('#code-type');
    if (codeTypeSelect) {
      codeTypeSelect.value = state.codeType;
    }
    
    // 设置位序
    const bitOrderSelect = container.querySelector('#bit-order');
    if (bitOrderSelect) {
      bitOrderSelect.value = state.bitOrder;
    }
    
    // 设置手动输入汉字
    const manualCharsInput = container.querySelector('#manual-chars');
    if (manualCharsInput) {
      manualCharsInput.value = state.manualChars;
    }
    
    // 设置生成方式
    const generateModeRadios = container.querySelectorAll('input[name="generate-mode"]');
    for (var i = 0; i < generateModeRadios.length; i++) {
      var radio = generateModeRadios[i];
      if (radio.value === state.generateMode) {
        radio.checked = true;
      }
    }
    
    // 设置清空现有字模
    const clearExistingCheckbox = container.querySelector('#clear-existing');
    if (clearExistingCheckbox) {
      clearExistingCheckbox.checked = state.clearExisting;
      // 更新重复处理选项的禁用状态
      updateDuplicateOptionsDisabled();
    }
    
    // 设置重复处理
    const duplicateHandlingRadios = container.querySelectorAll('input[name="duplicate-handling"]');
    for (var i = 0; i < duplicateHandlingRadios.length; i++) {
      var radio = duplicateHandlingRadios[i];
      if (radio.value === state.duplicateHandling) {
        radio.checked = true;
      }
    }
    
    // 设置记住选择
    const rememberChoiceCheckbox = container.querySelector('#remember-choice');
    if (rememberChoiceCheckbox) {
      rememberChoiceCheckbox.checked = state.rememberChoice;
    }
    
    // 设置状态
    updateStatus(state.status);
    
    // 设置生成按钮状态
    updateGenerateButton();
    
    // 设置找到的汉字
    renderFoundChars();
  };
  
  // 切换页面内容
  const switchPageContent = function(pageId) {
    // 更新状态
    state.activePage = pageId;
    
    // 获取所有页面元素
    const pages = [
      container.querySelector('#main-page'),
      container.querySelector('#test-page'),
      container.querySelector('#subpage1-page'),
      container.querySelector('#subpage2-page'),
      container.querySelector('#subpage3-page')
    ];
    
    // 隐藏所有页面
    for (var i = 0; i < pages.length; i++) {
      var page = pages[i];
      if (page) {
        page.style.display = 'none';
      }
    }
    
    // 显示当前活跃页面
    const activePage = container.querySelector('#' + pageId + '-page');
    if (activePage) {
      activePage.style.display = 'block';
    }
  };
  
  // 绑定事件监听器
  const bindEvents = function() {
    // 工程路径输入
    container.querySelector('#project-path').addEventListener('input', function(e) {
      state.projectPath = e.target.value;
    });
    
    // 浏览工程按钮
    container.querySelector('#browse-project').addEventListener('click', function() {
      handleBrowseProject();
    });
    
    // 字体名称选择
    container.querySelector('#font-name').addEventListener('change', function(e) {
      state.fontName = e.target.value;
    });
    
    // 字体大小输入
    container.querySelector('#font-size').addEventListener('input', function(e) {
      state.fontSize = parseInt(e.target.value);
    });
    
    // 取模方式选择
    container.querySelector('#mode').addEventListener('change', function(e) {
      state.mode = e.target.value;
    });
    
    // 码制选择
    container.querySelector('#code-type').addEventListener('change', function(e) {
      state.codeType = e.target.value;
    });
    
    // 位序选择
    container.querySelector('#bit-order').addEventListener('change', function(e) {
      state.bitOrder = e.target.value;
    });
    
    // 手动输入汉字
    container.querySelector('#manual-chars').addEventListener('input', function(e) {
      state.manualChars = e.target.value;
    });
    
    // 生成方式选择
    var generateModeRadios = container.querySelectorAll('input[name="generate-mode"]');
    for (var i = 0; i < generateModeRadios.length; i++) {
      var radio = generateModeRadios[i];
      radio.addEventListener('change', function(e) {
        state.generateMode = e.target.value;
      });
    };
    
    // 清空现有字模
    container.querySelector('#clear-existing').addEventListener('change', function(e) {
      state.clearExisting = e.target.checked;
      // 更新重复处理选项的禁用状态
      updateDuplicateOptionsDisabled();
    });
    
    // 重复处理选择
    var duplicateHandlingRadios = container.querySelectorAll('input[name="duplicate-handling"]');
    for (var i = 0; i < duplicateHandlingRadios.length; i++) {
      var radio = duplicateHandlingRadios[i];
      radio.addEventListener('change', function(e) {
        state.duplicateHandling = e.target.value;
      });
    };
    
    // 记住选择
    container.querySelector('#remember-choice').addEventListener('change', function(e) {
      state.rememberChoice = e.target.checked;
    });
    
    // 生成字模按钮
    container.querySelector('#generate-matrix').addEventListener('click', function() {
      handleGenerate();
    });
    
    // 保存配置按钮
    container.querySelector('#save-config').addEventListener('click', function() {
      handleSaveConfig();
    });
    
    // 加载测试数据按钮（保留，可能有用）
    const loadTestBtn = container.querySelector('#load-test-data');
    if (loadTestBtn) {
      loadTestBtn.addEventListener('click', function() {
        loadTestData();
      });
    }
    
    // 范围选择切换
    var generateRangeRadios = container.querySelectorAll('input[name="generate-range"]');
    for (var i = 0; i < generateRangeRadios.length; i++) {
      var radio = generateRangeRadios[i];
      radio.addEventListener('change', toggleCustomRange);
    };
    

    
    // 图片浏览按钮
    container.querySelector('#browse-image').addEventListener('click', function() {
      document.querySelector('#image-upload').click();
    });
    
    // 图片选择事件
    container.querySelector('#image-upload').addEventListener('change', handleImageSelect);
    
    // 生成图模按钮
    container.querySelector('#generate-matrix-data').addEventListener('click', function() {
      generateMatrixData();
    });
    
    // 清空数据按钮
    container.querySelector('#clear-matrix-data').addEventListener('click', function() {
      clearMatrixData();
    });
    
    // 保存到OLED_Data.c按钮
    container.querySelector('#save-matrix-data').addEventListener('click', function() {
      saveMatrixDataToFile();
    });
    
    // 监听页面切换事件
    container.addEventListener('plugin-page-changed', function(event) {
      const pageId = event.detail.pageId;
      switchPageContent(pageId);
    });
  };
  
  // 更新状态显示
  const updateStatus = function(message) {
    state.status = message;
    const statusElement = container.querySelector('#status-message');
    if (statusElement) {
      statusElement.textContent = message;
    }
  };
  
  // 更新生成按钮状态
  const updateGenerateButton = function() {
    const button = container.querySelector('#generate-matrix');
    if (button) {
      button.disabled = state.isGenerating;
      button.textContent = state.isGenerating ? '生成中...' : '生成字模';
    }
  };
  
  // 更新重复处理选项的禁用状态
  const updateDuplicateOptionsDisabled = function() {
    const disabled = state.clearExisting;
    var inputs = container.querySelectorAll('input[name="duplicate-handling"], #remember-choice');
    for (var i = 0; i < inputs.length; i++) {
      var input = inputs[i];
      input.disabled = disabled;
    };
  };
  
  // 更新工程路径显示
  const renderProjectPath = function() {
    const input = container.querySelector('#project-path');
    if (input) {
      input.value = state.projectPath;
    }
  };
  
  // 计算当前页显示的汉字
  const getCurrentPageChars = function() {
    // 确保state.foundChars是数组，避免undefined错误
    if (!Array.isArray(state.foundChars)) {
      return [];
    }
    const startIndex = (state.currentPage - 1) * state.pageSize;
    const endIndex = startIndex + state.pageSize;
    return state.foundChars.slice(startIndex, endIndex);
  };
  
  // 更新找到的汉字显示 - 支持分页
  const renderFoundChars = function() {
    console.log('开始调用renderFoundChars函数');
    console.log('当前state.foundChars:', state.foundChars);
    
    const section = container.querySelector('#found-chars-section');
    const charsContainer = container.querySelector('#found-chars');
    
    console.log('section元素:', section);
    console.log('charsContainer元素:', charsContainer);
    
    if (section && charsContainer) {
      if (state.foundChars && state.foundChars.length > 0) {
        console.log('state.foundChars有效，长度为:', state.foundChars.length);
        section.style.display = 'block';
        
        // 计算总页数
        state.totalPages = Math.ceil(state.foundChars.length / state.pageSize);
        console.log('计算得到总页数:', state.totalPages);
        
        // 获取当前页的数据
        console.log('调用getCurrentPageChars函数');
        const currentChars = getCurrentPageChars();
        console.log('getCurrentPageChars返回:', currentChars);
        
        // 渲染汉字
        charsContainer.innerHTML = '';
        
        console.log('开始遍历currentChars，长度为:', currentChars.length);
        
        // 修复：使用for循环时，正确获取当前字符并传递给事件
        for (let i = 0; i < currentChars.length; i++) {
          // 获取当前字符
          const currentChar = currentChars[i];
          console.log('遍历第', i + 1, '个字符:', currentChar);
          
          var charTag = document.createElement('span');
          charTag.className = 'char-tag';
          charTag.textContent = currentChar;
          charTag.style.cursor = 'pointer';
          charTag.style.userSelect = 'none'; // 防止默认的文本选择行为
          
          // 修复：直接绑定点击事件，使用let声明的currentChar避免闭包问题
          charTag.onclick = function() {
            console.log('点击了字符:', currentChar);
            generateCharMatrix(currentChar);
          };
          
          charsContainer.appendChild(charTag);
          console.log('字符标签已添加到容器');
        }
        
        console.log('遍历完成');
        
        // 渲染分页控件
        console.log('调用renderPagination函数');
        renderPagination();
        console.log('renderPagination函数调用完成');
      } else {
        console.log('state.foundChars无效或为空，隐藏找到的汉字区域');
        section.style.display = 'none';
        charsContainer.innerHTML = '';
        
        // 隐藏分页控件
        const paginationContainer = container.querySelector('#pagination');
        if (paginationContainer) {
          paginationContainer.style.display = 'none';
        }
      }
    }
    
    console.log('renderFoundChars函数调用完成');
  };
  
  // 生成汉字点阵数据并预览 - 使用实际字模数据
  const generateCharMatrix = function(char) {
    console.log('调用generateCharMatrix函数，字符:', char);
    if (!char) return;
    
    state.selectedChar = char;
    
    // 获取主页面内的预览容器，确保只操作主页面的预览元素
    const mainPage = container.querySelector('#main-page');
    const previewContainer = mainPage.querySelector('#matrix-preview');
    const previewSection = mainPage.querySelector('#matrix-preview-section');
    
    // 显示预览区域
    previewSection.style.display = 'block';
    
    // 清空预览容器
    previewContainer.innerHTML = '';
    
    // 创建预览标题
    const previewTitle = document.createElement('div');
    previewTitle.textContent = '汉字 "' + char + '" 点阵预览 (' + state.fontSize + 'x' + state.fontSize + ')';
    previewTitle.style.color = '#eee';
    previewTitle.style.textAlign = 'center';
    previewTitle.style.marginBottom = '1rem';
    previewContainer.appendChild(previewTitle);
    
    // 创建画布进行预览
    const canvas = document.createElement('canvas');
    const dotSize = 4;
    const margin = 2;
    const matrixSize = state.fontSize;
    canvas.width = matrixSize * (dotSize + margin);
    canvas.height = matrixSize * (dotSize + margin);
    canvas.style.border = '1px solid #444';
    
    const ctx = canvas.getContext('2d');
    
    if (ctx) {
      // 绘制背景
      ctx.fillStyle = '#3a3a3a';
      ctx.fillRect(0, 0, canvas.width, canvas.height);
      
      // 绘制点阵
      ctx.fillStyle = '#ffffff';
      
      // 获取该汉字的字模数据
      const bitmapData = state.charMatrixData[char];
      console.log('>>> 使用实际字模数据:', bitmapData ? '可用' : '不可用');
      
      if (bitmapData) {
        console.log('>>> 字模数据长度:', bitmapData.length);
        
        if (state.mode === '行列式') {
          // 行列式：按行扫描，每行8个像素为一个字节
          let byteIndex = 0;
          for (let row = 0; row < matrixSize; row += 8) {
            // 每行处理8行像素
            for (let col = 0; col < matrixSize; col++) {
              // 每列处理一个字节
              if (byteIndex < bitmapData.length) {
                const byte = bitmapData[byteIndex];
                // 处理一个字节的8个像素
                for (let bit = 0; bit < 8; bit++) {
                  const pixelRow = row + bit;
                  if (pixelRow < matrixSize) {
                    // 获取当前位值
                    const isSet = (byte & (1 << bit)) !== 0;
                    
                    // 阴码：0表示有墨，1表示无墨
                    // 阳码：1表示有墨，0表示无墨
                    // 所以需要根据阴码/阳码决定是否显示点
                    // 这里直接使用isSet的相反值，因为：
                    //   - 主进程生成字模时，阴码将有墨点设为0，阳码将有墨点设为1
                    //   - 但在预览中，我们需要显示有墨点，所以对于阴码需要取反
                    //   - 实际上，主进程生成的字节是用于存储的，而预览需要显示视觉效果
                    //   - 所以无论阴码阳码，我们都需要显示有墨的部分，即黑色像素
                    //   - 主进程在生成字节时，已经将黑色像素（有墨）转换为对应的值
                    //   - 但预览时，我们需要将有墨的部分显示为白点
                    //   - 所以正确的做法是：直接显示主进程生成的字节中的有墨部分
                    //   - 但实际上，主进程生成的字节中，有墨的部分在阳码中是1，阴码中是0
                    //   - 所以我们需要根据阴码/阳码来决定是否取反
                    //   - 但这里有个问题：主进程生成字模时使用的阴码/阳码可能与当前配置不同
                    //   - 所以正确的做法是：使用主进程生成时的阴码/阳码配置
                    //   - 但我们没有这个信息，所以只能假设主进程生成时使用的是当前配置
                    //   - 或者，我们可以直接取反，因为主进程生成的字节是阴码时，有墨为0，阳码时为1
                    //   - 而我们需要显示有墨的部分，所以无论阴码阳码，都需要显示有墨的部分
                    //   - 实际上，主进程生成的字节是：
                    //     - 阳码：1表示有墨，0表示无墨
                    //     - 阴码：0表示有墨，1表示无墨
                    //   - 所以在预览中，我们需要显示有墨的部分，即：
                    //     - 阳码：显示1的部分
                    //     - 阴码：显示0的部分
                    //   - 所以我们需要根据当前配置的阴码/阳码来决定是否取反
                    let shouldDraw = isSet;
                    if (state.codeType === '阴码') {
                      // 阴码：0表示有墨，所以需要取反
                      shouldDraw = !isSet;
                    }
                    
                    if (shouldDraw) {
                      ctx.fillRect(
                        col * (dotSize + margin),
                        pixelRow * (dotSize + margin),
                        dotSize,
                        dotSize
                      );
                    }
                  }
                }
                byteIndex++;
              }
            }
          }
        } else {
          // 列行式：按列扫描，每列8个像素为一个字节
          let byteIndex = 0;
          for (let col = 0; col < matrixSize; col++) {
            // 每列处理8列像素
            for (let rowBlock = 0; rowBlock < matrixSize; rowBlock += 8) {
              // 每8个像素为一个字节
              if (byteIndex < bitmapData.length) {
                const byte = bitmapData[byteIndex];
                // 处理一个字节的8个像素
                for (let bit = 0; bit < 8; bit++) {
                  const pixelRow = rowBlock + bit;
                  if (pixelRow < matrixSize) {
                    // 获取当前位值
                    const isSet = (byte & (1 << bit)) !== 0;
                    
                    // 处理阴码/阳码
                    let shouldDraw = isSet;
                    if (state.codeType === '阴码') {
                      // 阴码：0表示有墨，所以需要取反
                      shouldDraw = !isSet;
                    }
                    
                    if (shouldDraw) {
                      ctx.fillRect(
                        col * (dotSize + margin),
                        pixelRow * (dotSize + margin),
                        dotSize,
                        dotSize
                      );
                    }
                  }
                }
                byteIndex++;
              }
            }
          }
        }
      } else {
        // 如果没有字模数据，显示提示
        const noDataDiv = document.createElement('div');
        noDataDiv.textContent = '未找到该汉字的字模数据';
        noDataDiv.style.color = '#888';
        noDataDiv.style.textAlign = 'center';
        noDataDiv.style.padding = '2rem 0';
        previewContainer.appendChild(noDataDiv);
      }
      
      // 只在ctx存在时添加画布到预览容器
      previewContainer.appendChild(canvas);
    }
    
    console.log('>>> 字模预览绘制完成');
  };
  
  // 渲染分页控件
  const renderPagination = function() {
    console.log('开始调用renderPagination函数');
    
    // 查找或创建分页容器
    var paginationContainer = container.querySelector('#pagination');
    console.log('paginationContainer元素:', paginationContainer);
    
    if (!paginationContainer) {
      console.log('创建新的分页容器');
      paginationContainer = document.createElement('div');
      paginationContainer.id = 'pagination';
      paginationContainer.className = 'pagination';
      paginationContainer.style.cssText = "\n        display: flex;\n        justify-content: center;\n        align-items: center;\n        margin-top: 1rem;\n        gap: 0.5rem;\n      ";
      
      // 将分页容器添加到找到的汉字部分
      var foundCharsSection = container.querySelector('#found-chars-section');
      console.log('foundCharsSection元素:', foundCharsSection);
      
      if (foundCharsSection) {
        foundCharsSection.appendChild(paginationContainer);
        console.log('分页容器已添加到foundCharsSection');
      } else {
        console.error('foundCharsSection元素未找到');
      }
    }
    
    // 清空现有内容
    paginationContainer.innerHTML = '';
    console.log('分页容器内容已清空');
    
    // 首页按钮
    var firstButton = document.createElement('button');
    firstButton.textContent = '首页';
    firstButton.disabled = state.currentPage === 1;
    firstButton.style.cssText = "\n      padding: 0.4rem 0.8rem;\n      background-color: " + (state.currentPage === 1 ? '#444' : '#3a3a3a') + ";\n      border: 1px solid #555;\n      border-radius: 3px;\n      color: #eee;\n      cursor: " + (state.currentPage === 1 ? 'not-allowed' : 'pointer') + ";\n      opacity: " + (state.currentPage === 1 ? 0.6 : 1) + ";\n      font-size: 0.9rem;\n    ";
    firstButton.onclick = function() {
      state.currentPage = 1;
      renderFoundChars();
    };
    paginationContainer.appendChild(firstButton);
    
    // 上一页按钮
    var prevButton = document.createElement('button');
    prevButton.textContent = '上一页';
    prevButton.disabled = state.currentPage === 1;
    prevButton.style.cssText = "\n      padding: 0.4rem 0.8rem;\n      background-color: " + (state.currentPage === 1 ? '#444' : '#3a3a3a') + ";\n      border: 1px solid #555;\n      border-radius: 3px;\n      color: #eee;\n      cursor: " + (state.currentPage === 1 ? 'not-allowed' : 'pointer') + ";\n      opacity: " + (state.currentPage === 1 ? 0.6 : 1) + ";\n      font-size: 0.9rem;\n    ";
    prevButton.onclick = function() {
      if (state.currentPage > 1) {
        state.currentPage--;
        renderFoundChars();
      }
    };
    paginationContainer.appendChild(prevButton);
    
    // 页码显示
    var pageInfo = document.createElement('span');
    pageInfo.textContent = state.currentPage + ' / ' + state.totalPages;
    pageInfo.style.cssText = "\n      color: #eee;\n      font-size: 0.9rem;\n      padding: 0 1rem;\n    ";
    paginationContainer.appendChild(pageInfo);
    
    // 下一页按钮
    var nextButton = document.createElement('button');
    nextButton.textContent = '下一页';
    nextButton.disabled = state.currentPage === state.totalPages;
    nextButton.style.cssText = "\n      padding: 0.4rem 0.8rem;\n      background-color: " + (state.currentPage === state.totalPages ? '#444' : '#3a3a3a') + ";\n      border: 1px solid #555;\n      border-radius: 3px;\n      color: #eee;\n      cursor: " + (state.currentPage === state.totalPages ? 'not-allowed' : 'pointer') + ";\n      opacity: " + (state.currentPage === state.totalPages ? 0.6 : 1) + ";\n      font-size: 0.9rem;\n    ";
    nextButton.onclick = function() {
      if (state.currentPage < state.totalPages) {
        state.currentPage++;
        renderFoundChars();
      }
    };
    paginationContainer.appendChild(nextButton);
    
    // 末页按钮
    var lastButton = document.createElement('button');
    lastButton.textContent = '末页';
    lastButton.disabled = state.currentPage === state.totalPages;
    lastButton.style.cssText = "\n      padding: 0.4rem 0.8rem;\n      background-color: " + (state.currentPage === state.totalPages ? '#444' : '#3a3a3a') + ";\n      border: 1px solid #555;\n      border-radius: 3px;\n      color: #eee;\n      cursor: " + (state.currentPage === state.totalPages ? 'not-allowed' : 'pointer') + ";\n      opacity: " + (state.currentPage === state.totalPages ? 0.6 : 1) + ";\n      font-size: 0.9rem;\n    ";
    lastButton.onclick = function() {
      state.currentPage = state.totalPages;
      renderFoundChars();
    };
    paginationContainer.appendChild(lastButton);
    
    // 每页条数选择器
    var pageSizeContainer = document.createElement('div');
    pageSizeContainer.style.cssText = "\n      display: flex;\n      align-items: center;\n      gap: 0.5rem;\n      color: #eee;\n      font-size: 0.9rem;\n      margin-left: 1rem;\n    ";
    
    var pageSizeLabel = document.createElement('span');
    pageSizeLabel.textContent = '每页';
    pageSizeContainer.appendChild(pageSizeLabel);
    
    var pageSizeSelect = document.createElement('select');
    var pageSizeOptions = [10, 20, 50, 100];
    for (var i = 0; i < pageSizeOptions.length; i++) {
      var option = pageSizeOptions[i];
      var opt = document.createElement('option');
      opt.value = option;
      opt.textContent = option;
      if (option === state.pageSize) {
        opt.selected = true;
      }
      pageSizeSelect.appendChild(opt);
    }
    pageSizeSelect.style.cssText = "\n      padding: 0.3rem 0.5rem;\n      background-color: #3a3a3a;\n      border: 1px solid #555;\n      border-radius: 3px;\n      color: #eee;\n      font-size: 0.9rem;\n    ";
    pageSizeSelect.onchange = function(e) {
      state.pageSize = parseInt(e.target.value);
      state.currentPage = 1; // 重置到第一页
      renderFoundChars();
    };
    pageSizeContainer.appendChild(pageSizeSelect);
    
    var pageSizeText = document.createElement('span');
    pageSizeText.textContent = '条';
    pageSizeContainer.appendChild(pageSizeText);
    
    paginationContainer.appendChild(pageSizeContainer);
    
    // 显示分页控件
    paginationContainer.style.display = 'flex';
  };
  
  // 加载测试分页数据
  const loadTestData = function() {
    state.isLoadingTestData = true;
    updateTestDataStatus('加载测试数据中...');
    
    // 模拟API请求延迟
    setTimeout(function() {
      try {
        // 生成测试数据
        const total = 50;
        const start = (state.testCurrentPage - 1) * state.testPageSize;
        const end = start + state.testPageSize;
        const data = [];
        
        for (var i = start; i < end && i < total; i++) {
          data.push({
            id: i + 1,
            name: '测试项目 ' + (i + 1),
            content: '这是第 ' + (i + 1) + ' 个测试数据项，用于演示分页功能。',
            date: new Date().toLocaleDateString()
          });
        }
        
        state.testData = data;
        state.testTotalPages = Math.ceil(total / state.testPageSize);
        updateTestDataStatus('加载完成，共 ' + total + ' 条数据');
        renderTestData();
      } catch (error) {
        console.error('加载测试数据失败:', error);
        updateTestDataStatus('加载测试数据失败');
      } finally {
        state.isLoadingTestData = false;
      }
    }, 1000);
  };
  
  // 切换生成范围显示
  const toggleCustomRange = function() {
    const customRangeContainer = container.querySelector('#custom-range-container');
    const generateRange = container.querySelector('input[name="generate-range"]:checked');
    if (generateRange && generateRange.value === 'custom') {
      customRangeContainer.style.display = 'block';
    } else {
      customRangeContainer.style.display = 'none';
    }
  };
  

  
  // 处理图片选择
  const handleImageSelect = function(e) {
    const file = e.target.files[0];
    const selectedImageName = container.querySelector('#selected-image-name');
    const imagePreviewContainer = container.querySelector('#image-preview-container');
    const imagePreview = container.querySelector('#image-preview');
    
    if (file) {
      // 显示选择的文件名
      selectedImageName.textContent = file.name;
      
      // 读取图片并显示预览
      const reader = new FileReader();
      reader.onload = function(event) {
        imagePreview.src = event.target.result;
        imagePreviewContainer.style.display = 'block';
      };
      reader.readAsDataURL(file);
    } else {
      // 重置显示
      selectedImageName.textContent = '未选择图片';
      imagePreview.src = '';
      imagePreviewContainer.style.display = 'none';
    }
  };
  
  // 将点阵数据转换为十六进制数组
  const convertToHexArray = function(dotMatrix, matrixSize, mode) {
    // 添加调试信息：转换开始
    if (pluginApi && typeof pluginApi.debug === 'function') {
      pluginApi.debug('chinese-font-matrix-plugin', '开始转换点阵数据为十六进制数组', {
        mode: mode,
        matrixSize: matrixSize,
        totalDots: dotMatrix.length * matrixSize
      });
    }
    
    const hexArray = [];
    
    // 根据OLED驱动要求，固定使用纵向8点，高位在下，先从左到右，再从上到下格式
    // 纵向8点：每8个垂直像素组成一个字节
    // 高位在下：字节的最低位(bit 0)对应最上面的像素，最高位(bit 7)对应最下面的像素
    // 先从左到右，再从上到下：先处理一行的所有垂直字节，然后处理下一行
    
    // 添加调试信息：使用OLED驱动要求的格式
    if (pluginApi && typeof pluginApi.debug === 'function') {
      pluginApi.debug('chinese-font-matrix-plugin', '使用OLED驱动要求格式转换', {
        matrixSize: matrixSize,
        format: '纵向8点，高位在下，先从左到右，再从上到下',
        bytesPerRow: Math.ceil(matrixSize / 8),
        totalRows: matrixSize
      });
    }
    
    // 计算每行需要的字节数
    const bytesPerRow = Math.ceil(matrixSize / 8);
    
    // 逐行处理
    for (let row = 0; row < matrixSize; row += 8) {
      // 逐列处理
      for (let col = 0; col < matrixSize; col++) {
        let byte = 0;
        // 处理当前列的8个垂直像素（从row到row+7）
        for (let bit = 0; bit < 8; bit++) {
          const y = row + bit;
          if (y < matrixSize && dotMatrix[y][col]) {
            // 高位在下：bit 0对应最上面的像素，bit 7对应最下面的像素
            // 所以直接使用 bit 作为位移量
            byte |= (1 << bit);
          }
        }
        hexArray.push(byte.toString(16).padStart(2, '0'));
      }
    }
    
    // 添加调试信息：转换完成
    if (pluginApi && typeof pluginApi.debug === 'function') {
      pluginApi.debug('chinese-font-matrix-plugin', '点阵数据转换完成', {
        mode: mode,
        outputLength: hexArray.length,
        sampleOutput: hexArray.slice(0, 4).join(', ') + (hexArray.length > 4 ? ', ...' : ''),
        format: '纵向8点，高位在下，先从左到右，再从上到下'
      });
    }
    
    return hexArray;
  };
  
  // 生成图模数据
  const generateMatrixData = function() {
    const statusElement = container.querySelector('#test-data-status');
    statusElement.textContent = '正在生成图模...';
    statusElement.style.color = '#aaa';
    
    try {
      // 图片生成逻辑
      const imageUpload = container.querySelector('#image-upload');
      const matrixSize = parseInt(container.querySelector('#matrix-size').value);
      const arrayName = container.querySelector('#array-name').value || 'image_matrix';
      const mode = container.querySelector('#matrix-mode').value;
      
      // 添加调试信息 - 使用正确的pluginId
      if (pluginApi && typeof pluginApi.debug === 'function') {
        pluginApi.debug('chinese-font-matrix-plugin', '开始生成图模数据', {
          matrixSize: matrixSize,
          arrayName: arrayName,
          mode: mode,
          timestamp: new Date().toISOString()
        });
      }
      
      if (!imageUpload.files || imageUpload.files.length === 0) {
        statusElement.textContent = '请先选择图片';
        statusElement.style.color = '#ff6b6b';
        return;
      }
      
      const file = imageUpload.files[0];
      
      // 读取图片并生成点阵
      const reader = new FileReader();
      reader.onload = function(event) {
        const img = new Image();
        img.onload = function() {
          // 模拟生成图模数据
          setTimeout(function() {
            // 生成预览
            const previewContainer = container.querySelector('#matrix-preview');
            previewContainer.innerHTML = '';
            
            // 创建预览标题
            const previewTitle = document.createElement('div');
            previewTitle.textContent = '图片点阵预览 (' + matrixSize + 'x' + matrixSize + ')';
            previewTitle.style.color = '#eee';
            previewTitle.style.textAlign = 'center';
            previewTitle.style.marginBottom = '1rem';
            previewContainer.appendChild(previewTitle);
            
            // 创建画布进行预览
            const canvas = document.createElement('canvas');
            const dotSize = 4;
            const margin = 2;
            canvas.width = matrixSize * (dotSize + margin);
            canvas.height = matrixSize * (dotSize + margin);
            canvas.style.border = '1px solid #444';
            
            const ctx = canvas.getContext('2d');
            let dotMatrix = [];
            
            if (ctx) {
              // 绘制点阵
              ctx.fillStyle = '#3a3a3a';
              ctx.fillRect(0, 0, canvas.width, canvas.height);
              
              ctx.fillStyle = '#ffffff';
              
              // 创建临时画布用于图像处理
            const tempCanvas = document.createElement('canvas');
            tempCanvas.width = matrixSize;
            tempCanvas.height = matrixSize;
            const tempCtx = tempCanvas.getContext('2d');
            
            // 将原图缩放到点阵大小
            tempCtx.drawImage(img, 0, 0, matrixSize, matrixSize);
            
            // 添加调试信息：图像处理开始
            if (pluginApi && typeof pluginApi.debug === 'function') {
              pluginApi.debug('chinese-font-matrix-plugin', '图像处理开始', {
                originalImageSize: `${img.width}x${img.height}`,
                scaledSize: `${matrixSize}x${matrixSize}`,
                imageName: file.name
              });
            }
            
            // 获取图像数据
            const imageData = tempCtx.getImageData(0, 0, matrixSize, matrixSize);
            const data = imageData.data;
            
            // 初始化点阵矩阵
            dotMatrix = new Array(matrixSize).fill(null).map(() => new Array(matrixSize).fill(false));
            
            // 生成点阵（基于像素亮度）
            for (let y = 0; y < matrixSize; y++) {
              for (let x = 0; x < matrixSize; x++) {
                // 获取像素亮度（灰度值）
                const index = (y * matrixSize + x) * 4;
                const r = data[index];
                const g = data[index + 1];
                const b = data[index + 2];
                const brightness = (r + g + b) / 3;
                
                // 根据亮度决定是否绘制点
                if (brightness < 128) {
                  ctx.fillRect(
                    x * (dotSize + margin),
                    y * (dotSize + margin),
                    dotSize,
                    dotSize
                  );
                  dotMatrix[y][x] = true;
                }
              }
            }
            
            // 添加调试信息：点阵生成完成
            const activeDots = dotMatrix.flat().filter(dot => dot).length;
            if (pluginApi && typeof pluginApi.debug === 'function') {
              pluginApi.debug('chinese-font-matrix-plugin', '点阵生成完成', {
                totalDots: matrixSize * matrixSize,
                activeDots: activeDots,
                activeRatio: (activeDots / (matrixSize * matrixSize) * 100).toFixed(2) + '%'
              });
            }
            }
            
            previewContainer.appendChild(canvas);
            
            // 生成十六进制数组
              const hexArray = convertToHexArray(dotMatrix, matrixSize, mode);
              
              // 添加调试信息：十六进制数组转换完成
              if (pluginApi && typeof pluginApi.debug === 'function') {
                pluginApi.debug('chinese-font-matrix-plugin', '十六进制数组转换完成', {
                  arrayLength: hexArray.length,
                  arraySizeBytes: hexArray.length,
                  mode: mode,
                  sampleData: hexArray.slice(0, 8).join(', ') + (hexArray.length > 8 ? ', ...' : '')
                });
              }
              
              // 保存生成的数据到全局状态
              state.generatedImageData = {
                rawData: dotMatrix,
                hexArray: hexArray,
                arrayName: arrayName,
                width: matrixSize,
                height: matrixSize,
                mode: mode
              };
              
              // 显示生成结果
              const resultContainer = container.querySelector('#test-data-content');
              resultContainer.innerHTML = '<div style="background-color: #2d2d2d; border: 1px solid #444; border-radius: 4px; padding: 1rem;">' +
                '<h4 style="margin-top: 0; color: #eee;">生成结果</h4>' +
                '<p style="color: #aaa; margin: 0.5rem 0;">图片名称: <span style="color: #4ecdc4; font-weight: bold;">' + file.name + '</span></p>' +
                '<p style="color: #aaa; margin: 0.5rem 0;">点阵大小: <span style="color: #4ecdc4; font-weight: bold;">' + matrixSize + 'x' + matrixSize + '</span></p>' +
                '<p style="color: #aaa; margin: 0.5rem 0;">生成模式: <span style="color: #4ecdc4; font-weight: bold;">' + mode + '</span></p>' +
                '<p style="color: #aaa; margin: 0.5rem 0;">数组名称: <span style="color: #4ecdc4; font-weight: bold;">' + arrayName + '</span></p>' +
                '<p style="color: #aaa; margin: 0.5rem 0;">生成数据大小: <span style="color: #4ecdc4; font-weight: bold;">' + Math.ceil(matrixSize * matrixSize / 8) + ' 字节</span></p>' +
                '<div style="margin-top: 1rem;">' +
                  '<h5 style="color: #eee; margin: 0 0 0.5rem 0;">十六进制数组:</h5>' +
                  '<pre style="' +
                    'background-color: #1e1e1e;' +
                    'border: 1px solid #444;' +
                    'border-radius: 3px;' +
                    'padding: 0.8rem;' +
                    'color: #d4d4d4;' +
                    'font-family: monospace;' +
                    'font-size: 0.8rem;' +
                    'line-height: 1.4;' +
                    'overflow-x: auto;' +
                  '">' +
                    'const uint8_t ' + arrayName + '[] = {\n' +
                    '  ' + hexArray.join(', ') + '\n' +
                    '};\n' +
                    'const uint16_t ' + arrayName + '_width = ' + matrixSize + ';\n' +
                    'const uint16_t ' + arrayName + '_height = ' + matrixSize + ';\n' +
                  '</pre>' +
                '</div>' +
              '</div>';
              
              statusElement.textContent = '图片图模生成成功';
              statusElement.style.color = '#4ecdc4';
              
              // 添加调试信息：图模生成成功
              if (pluginApi && typeof pluginApi.debug === 'function') {
                pluginApi.debug('chinese-font-matrix-plugin', '图模生成成功', {
                  imageName: file.name,
                  matrixSize: `${matrixSize}x${matrixSize}`,
                  arrayName: arrayName,
                  mode: mode,
                  totalBytes: hexArray.length,
                  generatedAt: new Date().toISOString()
                });
              }
            }, 1500);
          };
          img.src = event.target.result;
        };
        reader.readAsDataURL(file);
    } catch (error) {
      console.error('生成图模失败:', error);
      const statusElement = container.querySelector('#test-data-status');
      statusElement.textContent = '生成图模失败: ' + (error.message || error);
      statusElement.style.color = '#ff6b6b';
    }
  };
  
  // 清空图模数据
  const clearMatrixData = function() {
    const statusElement = container.querySelector('#test-data-status');
    statusElement.textContent = '设置生成范围，点击"生成图模"开始生成';
    statusElement.style.color = '#888';
    
    const resultContainer = container.querySelector('#test-data-content');
    resultContainer.innerHTML = '';
    
    const previewContainer = container.querySelector('#matrix-preview');
    previewContainer.innerHTML = '<div style="color: #888;">选择图片并生成图模查看预览</div>';
    
    // 重置图片选择相关元素
    const selectedImageName = container.querySelector('#selected-image-name');
    const imagePreviewContainer = container.querySelector('#image-preview-container');
    const imagePreview = container.querySelector('#image-preview');
    const imageUpload = container.querySelector('#image-upload');
    
    if (selectedImageName) selectedImageName.textContent = '未选择图片';
    if (imagePreviewContainer) imagePreviewContainer.style.display = 'none';
    if (imagePreview) imagePreview.src = '';
    if (imageUpload) imageUpload.value = '';
    
    // 重置生成数据状态
    state.generatedImageData = null;
  };
  
  // 将图模数据保存到OLED_Data.c文件
  const saveMatrixDataToFile = function() {
    const statusElement = container.querySelector('#test-data-status');
    
    // 添加调试信息：开始保存图模数据
    if (pluginApi && typeof pluginApi.debug === 'function') {
      pluginApi.debug('chinese-font-matrix-plugin', '开始保存图模数据到文件', {
        hasGeneratedData: !!state.generatedImageData,
        projectPath: state.projectPath,
        timestamp: new Date().toISOString()
      });
    }
    
    // 检查是否已经生成了图模数据
    if (!state.generatedImageData) {
      statusElement.textContent = '请先生成图模数据';
      statusElement.style.color = '#ff6b6b';
      
      // 添加调试信息：保存失败 - 未生成图模数据
      if (pluginApi && typeof pluginApi.error === 'function') {
        pluginApi.error('chinese-font-matrix-plugin', '保存图模数据失败', {
          error: '未生成图模数据',
          code: 'NO_GENERATED_DATA'
        });
      }
      return;
    }
    
    // 检查工程路径
    if (!state.projectPath || state.projectPath === '.') {
      statusElement.textContent = '请先选择工程路径';
      statusElement.style.color = '#ff6b6b';
      
      // 添加调试信息：保存失败 - 未选择工程路径
      if (pluginApi && typeof pluginApi.error === 'function') {
        pluginApi.error('chinese-font-matrix-plugin', '保存图模数据失败', {
          error: '未选择工程路径',
          code: 'NO_PROJECT_PATH'
        });
      }
      return;
    }
    
    statusElement.textContent = '正在保存图模数据到OLED_Data.c文件...';
    statusElement.style.color = '#aaa';
    
    try {
      // 获取插件API
      if (!pluginApi) {
        throw new Error('插件API不可用');
      }
      
      // 获取生成的数据
      const { hexArray, arrayName, width, height, mode } = state.generatedImageData;
      
      // 添加调试信息：准备保存数据
      if (pluginApi && typeof pluginApi.debug === 'function') {
        pluginApi.debug('chinese-font-matrix-plugin', '准备保存图模数据', {
          arrayName: arrayName,
          matrixSize: `${width}x${height}`,
          mode: mode || '未知',
          totalBytes: hexArray.length,
          projectPath: state.projectPath
        });
      }
      
      // 转换十六进制字符串为实际数值数组
      const byteArray = hexArray.map(hex => parseInt(hex, 16));
      
      // 添加调试信息：开始调用API保存数据
      if (pluginApi && typeof pluginApi.debug === 'function') {
        pluginApi.debug('chinese-font-matrix-plugin', '开始调用API保存图模数据', {
          apiName: 'updateOledImageDataFile',
          requestData: {
            projectPath: state.projectPath,
            arrayName: arrayName,
            width: width,
            height: height,
            imageDataLength: byteArray.length,
            imageDataSample: byteArray.slice(0, 8).map(b => `0x${b.toString(16).padStart(2, '0')}`).join(', ')
          }
        });
      }
      
      // 调用插件API保存图模数据到OLED_Data.c文件
    const saveData = {
      projectPath: state.projectPath,
      imageData: byteArray,
      arrayName: arrayName,
      width: width,
      height: height
    };
    
    // 第一次调用，检查是否存在同名数组
    pluginApi.updateOledImageDataFile(saveData).then(function(result) {
      // 添加调试信息：API调用返回结果
      if (pluginApi && typeof pluginApi.debug === 'function') {
        pluginApi.debug('chinese-font-matrix-plugin', 'API调用返回结果', {
          apiName: 'updateOledImageDataFile',
          result: result,
          resultType: typeof result,
          timestamp: new Date().toISOString()
        });
      }
      
      // 检查是否数组已存在
      if (result && !result.success && result.error === 'ArrayAlreadyExists') {
        // 显示覆盖确认对话框
        const confirmResult = confirm(`数组 ${result.arrayName} 已存在于文件中，是否覆盖？`);
        
        if (confirmResult) {
          // 用户确认覆盖，再次调用API并添加overwrite标志
          return pluginApi.updateOledImageDataFile({
            ...saveData,
            overwrite: true
          });
        } else {
          // 用户取消覆盖
          statusElement.textContent = '保存已取消，未覆盖现有数组';
          statusElement.style.color = '#ffa500';
          
          // 添加调试信息：用户取消覆盖
          if (pluginApi && typeof pluginApi.info === 'function') {
            pluginApi.info('chinese-font-matrix-plugin', '用户取消覆盖现有数组', {
              arrayName: arrayName,
              projectPath: state.projectPath
            });
          }
          return null;
        }
      }
      
      return result;
    }).then(function(result) {
      // 处理最终结果（如果有的话）
      if (result && result.success) {
        statusElement.textContent = `图模数据已成功保存到OLED_Data.c文件，数组名称: ${arrayName}`;
        statusElement.style.color = '#4ecdc4';
        
        // 添加调试信息：保存成功
        if (pluginApi && typeof pluginApi.debug === 'function') {
          pluginApi.debug('chinese-font-matrix-plugin', '图模数据保存成功', {
            arrayName: arrayName,
            filePath: result.filePath || `${state.projectPath}/OLED_Data.c`,
            totalBytes: byteArray.length,
            savedAt: new Date().toISOString(),
            resultDetails: result,
            overwrite: result.overwrite
          });
        }
      } else if (result && !result.success) {
        statusElement.textContent = '保存图模数据失败: ' + (result.message || result.error);
        statusElement.style.color = '#ff6b6b';
        
        // 添加调试信息：保存失败
        if (pluginApi && typeof pluginApi.error === 'function') {
          pluginApi.error('chinese-font-matrix-plugin', '保存图模数据失败', {
            error: result.error,
            message: result.message,
            code: 'API_RETURN_FALSE',
            result: result,
            projectPath: state.projectPath,
            arrayName: arrayName
          });
        }
      }
    }).catch(function(error) {
      console.error('保存图模数据失败:', error);
      statusElement.textContent = '保存图模数据失败: ' + (error.message || error);
      statusElement.style.color = '#ff6b6b';
      
      // 添加调试信息：保存异常
      if (pluginApi && typeof pluginApi.error === 'function') {
        pluginApi.error('chinese-font-matrix-plugin', '保存图模数据异常', {
          error: error.message || String(error),
          stack: error.stack,
          arrayName: arrayName,
          filePath: `${state.projectPath}/OLED_Data.c`
        });
      }
    });
    } catch (error) {
      console.error('保存图模数据时发生异常:', error);
      statusElement.textContent = '保存图模数据时发生异常: ' + (error.message || error);
      statusElement.style.color = '#ff6b6b';
      
      // 添加调试信息：保存过程中发生异常
      if (pluginApi && typeof pluginApi.error === 'function') {
        pluginApi.error('chinese-font-matrix-plugin', '保存图模数据时发生异常', {
          error: error.message || String(error),
          stack: error.stack,
          stage: 'general_exception'
        });
      }
    }
  };
  
  // 更新测试数据状态
  const updateTestDataStatus = function(message) {
    const statusElement = container.querySelector('#test-data-status');
    if (statusElement) {
      statusElement.textContent = message;
    }
  };
  
  // 渲染测试数据
  const renderTestData = function() {
    const dataContainer = container.querySelector('#test-data-content');
    if (dataContainer) {
      dataContainer.innerHTML = '';
      
      if (state.testData.length > 0) {
        // 渲染数据列表
        const ul = document.createElement('ul');
        ul.style.cssText = `
          list-style: none;
          padding: 0;
          margin: 0;
          gap: 0.5rem;
          display: flex;
          flex-direction: column;
        `;
        
        for (var i = 0; i < state.testData.length; i++) {
          var item = state.testData[i];
          const li = document.createElement('li');
          li.style.cssText = "\n            background-color: #2d2d2d;\n            padding: 0.8rem;\n            border-radius: 4px;\n            border: 1px solid #444;\n          ";
          
          li.innerHTML = "\n            <h4 style=\"margin: 0 0 0.5rem 0; font-size: 1rem;\">" + item.name + "</h4>\n            <p style=\"margin: 0 0 0.5rem 0; color: #aaa; font-size: 0.9rem;\">" + item.content + "</p>\n            <div style=\"display: flex; justify-content: space-between; align-items: center;\">\n              <span style=\"font-size: 0.8rem; color: #888;\">ID: " + item.id + "</span>\n              <span style=\"font-size: 0.8rem; color: #888;\">" + item.date + "</span>\n            </div>\n          ";
          
          ul.appendChild(li);
        };
        
        dataContainer.appendChild(ul);
      } else {
        // 空数据提示
        const emptyElement = document.createElement('div');
        emptyElement.textContent = '暂无数据';
        emptyElement.style.cssText = `
          text-align: center;
          color: #888;
          padding: 1rem;
        `;
        
        dataContainer.appendChild(emptyElement);
      }
      
      // 渲染测试分页控件
      renderTestPagination();
    }
  };
  
  // 渲染测试分页控件
  const renderTestPagination = () => {
    // 查找或创建分页容器
    let paginationContainer = container.querySelector('#test-pagination');
    if (!paginationContainer) {
      paginationContainer = document.createElement('div');
      paginationContainer.id = 'test-pagination';
      paginationContainer.className = 'pagination';
      paginationContainer.style.cssText = `
        display: flex;
        justify-content: center;
        align-items: center;
        margin-top: 1rem;
        gap: 0.5rem;
      `;
      
      // 将分页容器添加到测试数据部分
      const testPage = container.querySelector('#test-page');
      testPage.appendChild(paginationContainer);
    }
    
    // 清空现有内容
    paginationContainer.innerHTML = '';
    
    // 首页按钮
    var firstButton = document.createElement('button');
    firstButton.textContent = '首页';
    firstButton.disabled = state.testCurrentPage === 1;
    firstButton.style.cssText = "padding: 0.4rem 0.8rem; " +
      "background-color: " + (state.testCurrentPage === 1 ? '#444' : '#3a3a3a') + "; " +
      "border: 1px solid #555; " +
      "border-radius: 3px; " +
      "color: #eee; " +
      "cursor: " + (state.testCurrentPage === 1 ? 'not-allowed' : 'pointer') + "; " +
      "opacity: " + (state.testCurrentPage === 1 ? 0.6 : 1) + "; " +
      "font-size: 0.9rem;";
    firstButton.onclick = function() {
      state.testCurrentPage = 1;
      loadTestData();
    };
    paginationContainer.appendChild(firstButton);
    
    // 上一页按钮
    var prevButton = document.createElement('button');
    prevButton.textContent = '上一页';
    prevButton.disabled = state.testCurrentPage === 1;
    prevButton.style.cssText = "padding: 0.4rem 0.8rem; " +
      "background-color: " + (state.testCurrentPage === 1 ? '#444' : '#3a3a3a') + "; " +
      "border: 1px solid #555; " +
      "border-radius: 3px; " +
      "color: #eee; " +
      "cursor: " + (state.testCurrentPage === 1 ? 'not-allowed' : 'pointer') + "; " +
      "opacity: " + (state.testCurrentPage === 1 ? 0.6 : 1) + "; " +
      "font-size: 0.9rem;";
    prevButton.onclick = function() {
      if (state.testCurrentPage > 1) {
        state.testCurrentPage--;
        loadTestData();
      }
    };
    paginationContainer.appendChild(prevButton);
    
    // 页码显示
    var pageInfo = document.createElement('span');
    pageInfo.textContent = state.testCurrentPage + ' / ' + state.testTotalPages;
    pageInfo.style.cssText = "color: #eee; " +
      "font-size: 0.9rem; " +
      "padding: 0 1rem;";
    paginationContainer.appendChild(pageInfo);
    
    // 下一页按钮
    var nextButton = document.createElement('button');
    nextButton.textContent = '下一页';
    nextButton.disabled = state.testCurrentPage === state.testTotalPages;
    nextButton.style.cssText = "padding: 0.4rem 0.8rem; " +
      "background-color: " + (state.testCurrentPage === state.testTotalPages ? '#444' : '#3a3a3a') + "; " +
      "border: 1px solid #555; " +
      "border-radius: 3px; " +
      "color: #eee; " +
      "cursor: " + (state.testCurrentPage === state.testTotalPages ? 'not-allowed' : 'pointer') + "; " +
      "opacity: " + (state.testCurrentPage === state.testTotalPages ? 0.6 : 1) + "; " +
      "font-size: 0.9rem;";
    nextButton.onclick = function() {
      if (state.testCurrentPage < state.testTotalPages) {
        state.testCurrentPage++;
        loadTestData();
      }
    };
    paginationContainer.appendChild(nextButton);
    
    // 末页按钮
    var lastButton = document.createElement('button');
    lastButton.textContent = '末页';
    lastButton.disabled = state.testCurrentPage === state.testTotalPages;
    lastButton.style.cssText = "padding: 0.4rem 0.8rem; " +
      "background-color: " + (state.testCurrentPage === state.testTotalPages ? '#444' : '#3a3a3a') + "; " +
      "border: 1px solid #555; " +
      "border-radius: 3px; " +
      "color: #eee; " +
      "cursor: " + (state.testCurrentPage === state.testTotalPages ? 'not-allowed' : 'pointer') + "; " +
      "opacity: " + (state.testCurrentPage === state.testTotalPages ? 0.6 : 1) + "; " +
      "font-size: 0.9rem;";
    lastButton.onclick = function() {
      state.testCurrentPage = state.testTotalPages;
      loadTestData();
    };
    paginationContainer.appendChild(lastButton);
    
    // 每页条数选择器
    var pageSizeContainer = document.createElement('div');
    pageSizeContainer.style.cssText = "display: flex; " +
      "align-items: center; " +
      "gap: 0.5rem; " +
      "color: #eee; " +
      "font-size: 0.9rem; " +
      "margin-left: 1rem;";
    
    var pageSizeLabel = document.createElement('span');
    pageSizeLabel.textContent = '每页';
    pageSizeContainer.appendChild(pageSizeLabel);
    
    var pageSizeSelect = document.createElement('select');
    var pageSizeOptions = [5, 10, 20, 50];
    for (var i = 0; i < pageSizeOptions.length; i++) {
      var option = pageSizeOptions[i];
      var opt = document.createElement('option');
      opt.value = option;
      opt.textContent = option;
      if (option === state.testPageSize) {
        opt.selected = true;
      }
      pageSizeSelect.appendChild(opt);
    }
    pageSizeSelect.style.cssText = "padding: 0.3rem 0.5rem; " +
      "background-color: #3a3a3a; " +
      "border: 1px solid #555; " +
      "border-radius: 3px; " +
      "color: #eee; " +
      "font-size: 0.9rem;";
    pageSizeSelect.onchange = function(e) {
      state.testPageSize = parseInt(e.target.value);
      state.testCurrentPage = 1; // 重置到第一页
      loadTestData();
    };
    pageSizeContainer.appendChild(pageSizeSelect);
    
    var pageSizeText = document.createElement('span');
    pageSizeText.textContent = '条';
    pageSizeContainer.appendChild(pageSizeText);
    
    paginationContainer.appendChild(pageSizeContainer);
    
    // 刷新按钮
    var refreshButton = document.createElement('button');
    refreshButton.textContent = '刷新数据';
    refreshButton.disabled = state.isLoadingTestData;
    refreshButton.style.cssText = "padding: 0.4rem 0.8rem; " +
      "background-color: #3a3a3a; " +
      "border: 1px solid #555; " +
      "border-radius: 3px; " +
      "color: #eee; " +
      "cursor: " + (state.isLoadingTestData ? 'not-allowed' : 'pointer') + "; " +
      "opacity: " + (state.isLoadingTestData ? 0.6 : 1) + "; " +
      "font-size: 0.9rem; " +
      "margin-left: 1rem;";
    refreshButton.onclick = loadTestData;
    paginationContainer.appendChild(refreshButton);
    
    // 显示分页控件
    paginationContainer.style.display = 'flex';
  };
  
  // 创建基本HTML结构 - 支持多页面切换
  var html = '<div class="chinese-font-matrix-plugin">' +
        '<div class="main-content" style="' +
          'background-color: #252525;' +
          'border-radius: 4px;' +
          'padding: 1rem;' +
        '">' +
          '<div id="main-page">' +
            '<div class="section">' +
              '<h3>工程目录</h3>' +
              '<div class="form-row">' +
                '<label for="project-path">路径:</label>' +
                '<input type="text" id="project-path" class="form-control" placeholder="请输入工程路径" />' +
                '<button id="browse-project" class="btn btn-secondary">浏览...</button>' +
              '</div>' +
            '</div>' +
            
            '<div class="section">' +
              '<h3>字体与取模</h3>' +
              '<div class="settings-grid" style="' +
                'display: flex;' +
                'flex-wrap: wrap;' +
                'gap: 1rem;' +
                'margin: 0 -0.5rem;' +
              '">' +
                '<div class="setting-item" style="' +
                  'flex: 1;' +
                  'min-width: 200px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="font-name">字体:</label>' +
                  '<select id="font-name" class="form-control"></select>' +
                '</div>' +
                '<div class="setting-item" style="' +
                  'flex: 1;' +
                  'min-width: 200px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="font-size">大小:</label>' +
                  '<input type="number" id="font-size" class="form-control" min="8" max="32" />' +
                '</div>' +
                '<div class="setting-item" style="' +
                  'flex: 1;' +
                  'min-width: 200px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="mode">方式:</label>' +
                  '<select id="mode" class="form-control">' +
                    '<option value="行列式">行列式</option>' +
                    '<option value="列行式">列行式</option>' +
                  '</select>' +
                '</div>' +
                '<div class="setting-item" style="' +
                  'flex: 1;' +
                  'min-width: 200px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="code-type">码制:</label>' +
                  '<select id="code-type" class="form-control">' +
                    '<option value="阴码">阴码</option>' +
                    '<option value="阳码">阳码</option>' +
                  '</select>' +
                '</div>' +
                '<div class="setting-item" style="' +
                  'flex: 1;' +
                  'min-width: 200px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="bit-order">位序:</label>' +
                  '<select id="bit-order" class="form-control">' +
                    '<option value="低位在前">低位在前</option>' +
                    '<option value="高位在前">高位在前</option>' +
                  '</select>' +
                '</div>' +
              '</div>' +
            '</div>' +
            
            '<div class="section">' +
              '<h3>生成与字模处理</h3>' +
              '<div class="settings-grid" style="' +
                'display: flex;' +
                'flex-wrap: wrap;' +
                'gap: 1rem;' +
                'margin: 0 -0.5rem;' +
              '">' +
                '<div class="setting-item full-width" style="' +
                  'flex: 1 1 100%;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="manual-chars">输入汉字:</label>' +
                  '<input type="text" id="manual-chars" class="form-control" placeholder="请输入要生成的汉字" />' +
                '</div>' +
                '<div class="setting-item half-width" style="' +
                  'flex: 1;' +
                  'min-width: 300px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label>生成方式:</label>' +
                  '<div class="radio-group compact">' +
                    '<label>' +
                      '<input type="radio" name="generate-mode" value="search" />' +
                      '仅搜索代码' +
                    '</label>' +
                    '<label>' +
                      '<input type="radio" name="generate-mode" value="manual" />' +
                      '仅手动输入' +
                    '</label>' +
                    '<label>' +
                      '<input type="radio" name="generate-mode" value="both" />' +
                      '两者结合' +
                    '</label>' +
                  '</div>' +
                '</div>' +
                '<div class="setting-item half-width" style="' +
                  'flex: 1;' +
                  'min-width: 300px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label>字模处理:</label>' +
                  '<div class="radio-group compact">' +
                    '<label>' +
                      '<input type="checkbox" id="clear-existing" />' +
                      '清空现有字模' +
                    '</label>' +
                    '<div style="margin-left: 20px;">' +
                      '<label>' +
                        '<input type="radio" name="duplicate-handling" value="ask" />' +
                        '重复: 询问' +
                      '</label>' +
                      '<label>' +
                        '<input type="radio" name="duplicate-handling" value="overwrite" />' +
                        '重复: 覆盖' +
                      '</label>' +
                      '<label>' +
                        '<input type="radio" name="duplicate-handling" value="keep" />' +
                        '重复: 保留' +
                      '</label>' +
                      '<label>' +
                        '<input type="checkbox" id="remember-choice" />' +
                        '记住选择' +
                      '</label>' +
                    '</div>' +
                  '</div>' +
                '</div>' +
              '</div>' +
            '</div>' +
            
            '<div class="section status-section">' +
              '<div id="status-message" class="status-message">加载配置中...</div>' +
            '</div>' +
            
            '<div class="button-section">' +
              '<div class="button-group">' +
                '<button id="generate-matrix" class="btn btn-primary">生成字模</button>' +
                '<button id="save-config" class="btn btn-secondary">保存配置</button>' +
              '</div>' +
            '</div>' +
            
            '<div id="found-chars-section" class="section" style="display: none;">' +
              '<h3>找到的汉字</h3>' +
              '<div id="found-chars" class="found-chars"></div>' +
              
              '<div id="matrix-preview-section" style="' +
                'margin-top: 1rem;' +
                'padding-top: 1rem;' +
                'border-top: 1px solid #444;' +
                'display: none;' +
              '">' +
                '<h3>字模预览</h3>' +
                '<div id="matrix-preview" style="' +
                  'background-color: #2d2d2d;' +
                  'border: 1px solid #444;' +
                  'border-radius: 4px;' +
                  'padding: 1rem;' +
                  'margin-top: 1rem;' +
                  'display: flex;' +
                  'justify-content: center;' +
                  'align-items: center;' +
                  'min-height: 200px;' +
                '">' +
                  '<div style="color: #888;">点击找到的汉字查看字模预览</div>' +
                '</div>' +
              '</div>' +
            '</div>' +
            
            '<div class="plugin-footer compact">' +
              '<h3>使用说明</h3>' +
              '<ul class="compact">' +
                '<li>选择工程目录，自动搜索User和HardWare文件夹</li>' +
                '<li>设置字体和取模参数</li>' +
                '<li>选择生成方式，点击"生成字模"开始生成</li>' +
                '<li>生成的字模数据将替换到OLED_Data.c文件中</li>' +
              '</ul>' +
            '</div>' +
          '</div>' +
          
          '<div id="test-page" style="display: none;">' +
            '<div class="section">' +
              '<h3>图模生成</h3>' +
              '<div style="margin-bottom: 1rem;">' +
                  '<p style="color: #aaa; font-size: 0.9rem;">生成图片点阵图模数据，支持自定义点阵大小和生成模式。</p>' +
                  '<p id="test-data-status" style="color: #888; font-size: 0.8rem; margin-top: 0.5rem;">选择图片，设置参数，点击"生成图模"开始生成</p>' +
                '</div>' +
              
              '<div class="settings-grid" style="' +
                'display: flex;' +
                'flex-wrap: wrap;' +
                'gap: 1rem;' +
                'margin: 0 -0.5rem;' +
              '">' +
                '<div class="setting-item full-width" style="' +
                  'flex: 1 1 100%;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="image-upload">选择图片:</label>' +
                  '<div style="display: flex; align-items: center; gap: 0.5rem;">' +
                    '<input type="file" id="image-upload" accept="image/*" style="display: none;" />' +
                    '<button id="browse-image" class="btn btn-secondary">浏览图片...</button>' +
                    '<span id="selected-image-name" style="color: #aaa; font-size: 0.9rem;">未选择图片</span>' +
                  '</div>' +
                  '<div id="image-preview-container" style="' +
                    'margin-top: 1rem;' +
                    'border: 1px solid #444;' +
                    'border-radius: 4px;' +
                    'padding: 1rem;' +
                    'background-color: #2d2d2d;' +
                    'display: none;' +
                  '">' +
                    '<img id="image-preview" style="' +
                      'max-width: 100%;' +
                      'max-height: 200px;' +
                      'object-fit: contain;' +
                    '" />' +
                  '</div>' +
                '</div>' +
                
                '<div class="setting-item" style="' +
                  'flex: 1;' +
                  'min-width: 200px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="matrix-size">点阵大小:</label>' +
                  '<select id="matrix-size" class="form-control">' +
                    '<option value="8">8x8</option>' +
                    '<option value="16">16x16</option>' +
                    '<option value="32" selected>32x32</option>' +
                    '<option value="64">64x64</option>' +
                    '<option value="128">128x128</option>' +
                  '</select>' +
                '</div>' +
                
                '<div class="setting-item" style="' +
                  'flex: 1;' +
                  'min-width: 200px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="array-name">数组名称:</label>' +
                  '<input type="text" id="array-name" class="form-control" placeholder="请输入数组名称" value="image_matrix" />' +
                '</div>' +
                
                '<div class="setting-item" style="' +
                  'flex: 1;' +
                  'min-width: 200px;' +
                  'padding: 0 0.5rem;' +
                '">' +
                  '<label for="matrix-mode">生成模式:</label>' +
                  '<select id="matrix-mode" class="form-control">' +
                    '<option value="行列式" selected>行列式</option>' +
                    '<option value="列行式">列行式</option>' +
                  '</select>' +
                '</div>' +
              '</div>' +
              
              '<div style="margin-top: 1rem; margin-bottom: 1rem;">' +
                '<button id="generate-matrix-data" class="btn btn-primary">' +
                  '生成图模' +
                '</button>' +
                '<button id="clear-matrix-data" class="btn btn-secondary" style="margin-left: 0.5rem;">' +
                  '清空数据' +
                '</button>' +
                '<button id="save-matrix-data" class="btn btn-success" style="margin-left: 0.5rem;">' +
                  '保存到工程' +
                '</button>' +
              '</div>' +
              
              '<div id="test-data-content" class="test-data-content"></div>' +
              
              '<div class="section">' +
                '<h3>预览</h3>' +
                '<div id="matrix-preview" style="' +
                  'background-color: #2d2d2d;' +
                  'border: 1px solid #444;' +
                  'border-radius: 4px;' +
                  'padding: 1rem;' +
                  'margin-top: 1rem;' +
                  'display: flex;' +
                  'justify-content: center;' +
                  'align-items: center;' +
                  'min-height: 200px;' +
                '">' +
                  '<div style="color: #888;">选择汉字并生成图模查看预览</div>' +
                '</div>' +
              '</div>' +
            '</div>' +
          '</div>' +
          
          '<div id="subpage1-page" style="display: none;">' +
            '<div class="section">' +
              '<h3>子页面1</h3>' +
              '<div style="margin-bottom: 1rem;">' +
                '<p style="color: #aaa; font-size: 0.9rem;">这是中文字模插件的第一个子页面，用于测试多页面功能。</p>' +
                '<p style="color: #888; font-size: 0.8rem; margin-top: 0.5rem;">您可以在这个页面添加更多的功能模块或测试内容。</p>' +
              '</div>' +
              '<div class="settings-grid">' +
                '<div class="setting-item">' +
                  '<label for="subpage1-input1">测试输入1:</label>' +
                  '<input type="text" id="subpage1-input1" class="form-control" placeholder="请输入测试内容" />' +
                '</div>' +
                '<div class="setting-item">' +
                  '<label for="subpage1-select1">测试选择1:</label>' +
                  '<select id="subpage1-select1" class="form-control">' +
                    '<option value="option1">选项1</option>' +
                    '<option value="option2">选项2</option>' +
                    '<option value="option3">选项3</option>' +
                  '</select>' +
                '</div>' +
              '</div>' +
              '<div style="margin-top: 1rem;">' +
                '<button id="subpage1-button" class="btn btn-primary">' +
                  '测试按钮1' +
                '</button>' +
              '</div>' +
            '</div>' +
          '</div>' +
          
          '<div id="subpage2-page" style="display: none;">' +
            '<div class="section">' +
              '<h3>子页面2</h3>' +
              '<div style="margin-bottom: 1rem;">' +
                '<p style="color: #aaa; font-size: 0.9rem;">这是中文字模插件的第二个子页面，用于测试多页面功能。</p>' +
                '<p style="color: #888; font-size: 0.8rem; margin-top: 0.5rem;">这个页面展示了不同的布局和内容结构。</p>' +
              '</div>' +
              '<div class="settings-grid">' +
                '<div class="setting-item full-width">' +
                  '<label for="subpage2-textarea">测试文本域:</label>' +
                  '<textarea id="subpage2-textarea" class="form-control" rows="5" placeholder="请输入多行文本"></textarea>' +
                '</div>' +
                '<div class="setting-item">' +
                  '<label>测试复选框:</label>' +
                  '<div class="checkbox-group">' +
                    '<label>' +
                      '<input type="checkbox" name="subpage2-checkbox" value="checkbox1" /> 复选框1' +
                    '</label>' +
                    '<label>' +
                      '<input type="checkbox" name="subpage2-checkbox" value="checkbox2" /> 复选框2' +
                    '</label>' +
                    '<label>' +
                      '<input type="checkbox" name="subpage2-checkbox" value="checkbox3" /> 复选框3' +
                    '</label>' +
                  '</div>' +
                '</div>' +
              '</div>' +
            '</div>' +
          '</div>' +
          
          '<div id="subpage3-page" style="display: none;">' +
            '<div class="section">' +
              '<h3>子页面3</h3>' +
              '<div style="margin-bottom: 1rem;">' +
                '<p style="color: #aaa; font-size: 0.9rem;">这是中文字模插件的第三个子页面，用于测试多页面功能。</p>' +
                '<p style="color: #888; font-size: 0.8rem; margin-top: 0.5rem;">这个页面展示了更多的测试内容和交互元素。</p>' +
              '</div>' +
              '<div class="settings-grid">' +
                '<div class="setting-item">' +
                  '<label for="subpage3-number">测试数字输入:</label>' +
                  '<input type="number" id="subpage3-number" class="form-control" min="1" max="100" placeholder="请输入数字" />' +
                '</div>' +
                '<div class="setting-item">' +
                  '<label>测试单选按钮:</label>' +
                  '<div class="radio-group">' +
                    '<label>' +
                      '<input type="radio" name="subpage3-radio" value="radio1" /> 单选按钮1' +
                    '</label>' +
                    '<label>' +
                      '<input type="radio" name="subpage3-radio" value="radio2" /> 单选按钮2' +
                    '</label>' +
                    '<label>' +
                      '<input type="radio" name="subpage3-radio" value="radio3" /> 单选按钮3' +
                    '</label>' +
                  '</div>' +
                '</div>' +
              '</div>' +
              '<div style="margin-top: 1rem;">' +
                '<div class="button-group">' +
                  '<button id="subpage3-button1" class="btn btn-primary">' +
                    '测试按钮A' +
                  '</button>' +
                  '<button id="subpage3-button2" class="btn btn-secondary">' +
                    '测试按钮B' +
                  '</button>' +
                '</div>' +
              '</div>' +
            '</div>' +
          '</div>' +
        '</div>' +
      '</div>'
  
  // 将HTML内容添加到容器中
  container.innerHTML = html;
  
  // 绑定事件
  bindEvents();
  
  // 加载配置，使用回调函数确保配置加载完成后再设置UI
  const loadConfigAndUpdateUI = function() {
    // 加载配置
    loadConfig(function() {
      try {
        // 填充字体选项
        fillFontOptions();
        // 设置初始值
        setInitialValues();
        // 更新状态
        updateStatus('就绪');
        // 显示初始页面
        switchPageContent('main');
      } catch (error) {
        console.error('加载配置失败:', error);
        updateStatus('加载配置失败');
      }
    });
  };
  
  // 执行异步加载配置和更新UI
  loadConfigAndUpdateUI();
  
  // 返回DOM元素
  return container;
};

// 使用ES模块的export default，确保动态import能正确加载
export default ChineseFontMatrixPlugin;
