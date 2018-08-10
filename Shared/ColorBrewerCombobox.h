#if !defined(AFX_COLORBREWERCOMBOBOX_H__87AB439E_A5C6_4486_9985_0B5DC1D0F530__INCLUDED_)
#define AFX_COLORBREWERCOMBOBOX_H__87AB439E_A5C6_4486_9985_0B5DC1D0F530__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// contenttypecombobox.h : header file
//

#include "..\shared\ownerdrawcomboboxbase.h"
#include "..\3rdParty\ColorBrewer.h"

/////////////////////////////////////////////////////////////////////////////
// CContentTypeComboBox window

class CContentMgr;
class CONTENTFORMAT;

class CColorBrewerComboBox : public COwnerdrawComboBoxBase
{
// Construction
public:
	CColorBrewerComboBox(DWORD dwBrewerFlags = 0);

	BOOL Initialize();
	BOOL Initialize(COLORBREWER_PALETTETYPE nType);
	BOOL Initialize(COLORBREWER_PALETTETYPE nType, int nNumColors);
	BOOL Initialize(int nNumColors);
	BOOL Initialize(LPCTSTR szName, BOOL bPartial = TRUE);

	int GetSelectedPalette(CColorBrewerPalette& palette) const;
	int SetSelectedPalette(const CColorBrewerPalette& palette);
	void SetCurSel(int nSel);
	int GetCurSel() const;	

// Attributes
protected:
	CColorBrewer m_brewer;
	CColorBrewerPaletteArray m_aPalettes;
	int m_nInitSel;

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CContentTypeComboBox)
	protected:
	virtual void PreSubclassWindow();
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CColorBrewerComboBox();

	// Generated message map functions
protected:
	//{{AFX_MSG(CContentTypeComboBox)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()

	void FillCombo();
	void RebuildCombo();
	void DrawItemText(CDC& dc, const CRect& rect, int nItem, UINT nItemState,
						DWORD dwItemData, const CString& sItem, BOOL bList, COLORREF crText);	
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_COLORBREWERCOMBOBOX_H__87AB439E_A5C6_4486_9985_0B5DC1D0F530__INCLUDED_)
